using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using GeeYeangSore.DTO.User;
using GeeYeangSore.Settings; //加入SMTP設定
using Microsoft.Extensions.Options; //載入appsettings.json

namespace GeeYeangSore.APIControllers;

[ApiController]
[Route("api/[controller]")]
public class EmailTokenController : ControllerBase
{
    private readonly GeeYeangSoreContext _context;
    private readonly SmtpSettings _smtp;

    public EmailTokenController(GeeYeangSoreContext context, IOptions<SmtpSettings> smtpOptions)
    {
        _context = context;
        _smtp = smtpOptions.Value;
    }

    // ✅ 發送驗證信
    [HttpPost("send-token")]
    public async Task<IActionResult> SendToken([FromBody] SendTokenDto dto)
    {
        try
        {

            // 取得使用者信箱
            var userEmail = dto.UserEmail;

            // 1. 產生驗證碼
            string rawToken = GenerateRandomToken();
            string salt = Guid.NewGuid().ToString();
            string hashedToken = HashToken(rawToken + salt);

            // 2. 寄送信件
            await SendEmailAsync(dto.UserEmail, rawToken);

            // 3. 儲存驗證碼至資料庫
            var existingToken = _context.HEmailTokens
                .Where(x => x.HUserEmail == userEmail && !x.HIsUsed)
                .OrderByDescending(x => x.HCreatedAt)
                .FirstOrDefault();

            if (existingToken != null)
            {
                // ✅ 更新原有資料
                existingToken.HEmailToken1 = hashedToken;
                existingToken.HEmailSalt = salt;
                existingToken.HResetExpiresAt = DateTime.UtcNow.AddMinutes(10);
                existingToken.HCreatedAt = DateTime.UtcNow;
                existingToken.HRequestIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                existingToken.HTokenType = "Register"; // ✅ 註冊用途
            }
            else
            {
                // ✅ 沒有就新增一筆
                var token = new HEmailToken
                {
                    HUserEmail = userEmail,
                    HEmailToken1 = hashedToken,
                    HEmailSalt = salt,
                    HResetExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    HIsUsed = false,
                    HCreatedAt = DateTime.UtcNow,
                    HRequestIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    HTokenType = "Register"
                };
                _context.HEmailTokens.Add(token);
            }

            await _context.SaveChangesAsync();


            return Ok("驗證信已寄出");
        }
        catch (Exception ex)
        {
            Console.WriteLine("發送註冊驗證信錯誤：" + ex.ToString()); // ✅ 印出 inner exception
            return StatusCode(500, new { success = false, message = "寄送失敗", error = ex.ToString() });
        }

    }

    // ✅ 比對驗證碼
    [HttpPost("verify-token")]
    public IActionResult VerifyToken([FromBody] VerifyTokenDto dto)
    {
        try
        {
            var record = _context.HEmailTokens
                .Where(x => x.HUserEmail == dto.UserEmail &&
                            !x.HIsUsed &&
                            x.HResetExpiresAt > DateTime.UtcNow &&
                            x.HTokenType == "Register")
                .OrderByDescending(x => x.HCreatedAt)
                .FirstOrDefault();

            if (record == null)
                return BadRequest("查無驗證資料或已過期");

            string hashedInput = HashToken(dto.InputToken + record.HEmailSalt);
            if (hashedInput != record.HEmailToken1)
                return BadRequest("驗證碼錯誤");

            record.HIsUsed = true;
            record.HUsedAt = DateTime.UtcNow;
            _context.SaveChanges();

            return Ok("驗證成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine("驗證過程發生錯誤：" + ex.ToString());
            return StatusCode(500, new { success = false, message = "驗證失敗", error = ex.ToString() });
        }
    }



    // 隨機6位驗證碼
    private string GenerateRandomToken()
    {
        var rnd = new Random();
        return rnd.Next(100000, 999999).ToString();
    }

    // 雜湊
    private string HashToken(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    // 寄送信件
    private async Task SendEmailAsync(string toEmail, string token)
    {
        try
        {
            var fromEmail = _smtp.FromEmail;           // 使用設定中的寄件者
            var appPassword = _smtp.AppPassword;       // 使用設定中的密碼

            var message = new MailMessage();
            message.From = new MailAddress(fromEmail, "居研所租屋平台");
            message.To.Add(new MailAddress(toEmail));
            message.Subject = "居研所租屋平台｜驗證碼寄送";

            // 美化後的 HTML 郵件內容
            message.Body = $@"
    <div style='font-family:Arial,sans-serif; max-width:600px; margin:auto; padding:20px; border:1px solid #ddd; border-radius:10px;'>
        <h2 style='color:#2c3e50;'>居研所租屋平台</h2>
        <p>親愛的使用者您好，</p>
        <p>您的驗證碼如下，請於 <strong>10 分鐘內</strong> 完成輸入驗證：</p>
        <div style='font-size:32px; font-weight:bold; color:#e74c3c; margin:20px 0;'>{token}</div>
        <p style='font-size:14px; color:#888;'>※ 此為系統自動發送信件，請勿直接回覆。</p>
        <hr/>
        <p style='font-size:12px; color:#aaa;'>居研所租屋平台 © {DateTime.Now.Year}</p>
    </div>";

            message.IsBodyHtml = true; // ✅ 設定為 HTML 格式

            using var client = new SmtpClient(_smtp.Host, _smtp.Port)  // 使用設定中的主機與埠號
            {
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("SMTP 發信錯誤：" + ex.Message);
            // ⚠️ 請勿在這裡重新丟出例外，否則還是會讓 Swagger 掛掉
        }
    }
}
