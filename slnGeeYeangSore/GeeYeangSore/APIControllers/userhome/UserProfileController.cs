using GeeYeangSore.APIControllers.Session;
using GeeYeangSore.Models;
using GeeYeangSore.DTO.User;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;


namespace GeeYeangSore.APIControllers.userhome;

[Route("api/[controller]")]
[ApiController]
public class UserProfileController : BaseController
{
    private readonly IWebHostEnvironment _env;

    public UserProfileController(GeeYeangSoreContext db, IWebHostEnvironment env) : base(db)
    {
        _env = env;
    }


    // ① 取得目前登入者資料
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var tenant = GetCurrentTenant();
        if (tenant == null) return Unauthorized();

        var data = new
        {
            tenant.HUserName,
            tenant.HBirthday,
            Gender = tenant.HGender == true ? "male" : "female",
            Address = tenant.HAddress,
            tenant.HPhoneNumber,
            tenant.HEmail,
            Avatar = tenant.HImages,
            IsGoogleLogin = tenant.HPassword == null
        };

        return Ok(data);
    }

    // ② 上傳暫存頭像（只存至 wwwroot/temp-avatars）
    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("未提供檔案");

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"temp_{Guid.NewGuid()}{ext}";
        var folderPath = Path.Combine(_env.WebRootPath, "temp-avatars");
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/temp-avatars/{fileName}";
        return Ok(new { avatarUrl = url });
    }

    // ③ 儲存變更（包含密碼驗證與頭像）
    [HttpPost("save-profile")]
    public IActionResult SaveProfile([FromBody] UpdateProfileRequest dto)
    {
        var tenant = GetCurrentTenant();
        if (tenant == null) return Unauthorized();

        if (!string.IsNullOrEmpty(dto.Password))
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("密碼與確認密碼不一致");

            var salt = Guid.NewGuid().ToString();
            var hashed = HashPassword(dto.Password, salt);
            tenant.HPassword = hashed;
            tenant.HSalt = salt;
        }

        tenant.HUserName = dto.Name;
        tenant.HBirthday = dto.Birthday;
        tenant.HGender = dto.Gender == "male";
        tenant.HAddress = $"{dto.City} {dto.Address}".Trim();
        tenant.HPhoneNumber = dto.Phone;
        tenant.HImages = dto.Avatar;
        tenant.HUpdateAt = DateTime.Now;

        _db.SaveChanges();

        return Ok("更新成功");
    }

    // 密碼雜湊
    private string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var salted = $"{salt}{password}";
        var bytes = Encoding.UTF8.GetBytes(salted);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
