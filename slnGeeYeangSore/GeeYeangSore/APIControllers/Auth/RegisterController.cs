using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using GeeYeangSore.DTO.User;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GeeYeangSore.APIControllers.Auth
{
    [ApiController]
    [Route("api/[controller]")] // 對應為 /api/register
    public class RegisterController : BaseController
    {
        public RegisterController(GeeYeangSoreContext db) : base(db) { }

        [HttpPost]
        public IActionResult Register([FromBody] RegisterRequestDto dto)
        {
            try
            {
                // ✅ 基本欄位檢查
                if (string.IsNullOrWhiteSpace(dto.UserName))
                    return BadRequest(new { success = false, message = "請輸入姓名" });

                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new { success = false, message = "請輸入完整資訊" });

                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[^\s]{10,}$"))
                    return BadRequest(new { success = false, message = "密碼需至少10字元，包含大小寫英文字母、數字、特殊符號，且不得包含空白" });

                if (!dto.IsAgreePolicy)
                    return BadRequest(new { success = false, message = "請先勾選同意隱私權政策" });

                // ✅ 驗證碼比對（使用雜湊 + salt）
                var tokenRecord = _db.HEmailTokens
                    .Where(t =>
                        t.HUserEmail == dto.Email &&
                        t.HTokenType == "Register" &&
                        t.HResetExpiresAt > DateTime.UtcNow &&
                        !t.HIsUsed)
                    .OrderByDescending(t => t.HCreatedAt)
                    .FirstOrDefault();

                if (tokenRecord == null)
                {
                    return BadRequest(new { success = false, message = "驗證碼錯誤、過期或已使用" });
                }

                // 🐥 雜湊比對輸入的驗證碼
                string hashedInput = HashToken(dto.VerificationCode + tokenRecord.HEmailSalt);
                if (hashedInput != tokenRecord.HEmailToken1)
                {
                    return BadRequest(new { success = false, message = "驗證碼錯誤" });
                }

                // ✅ 標記驗證碼為已使用
                tokenRecord.HIsUsed = true;
                tokenRecord.HUsedAt = DateTime.UtcNow;
                _db.SaveChanges();

                // ✅ 信箱重複檢查
                if (_db.HTenants.Any(t => t.HEmail == dto.Email))
                    return BadRequest(new { success = false, message = "此信箱已註冊" });

                // ✅ 雜湊密碼
                var salt = PasswordHasher.GenerateSalt();
                var hash = PasswordHasher.HashPassword(dto.Password, salt);

                // ✅ 建立房客資料
                HTenant newTenant = new HTenant
                {
                    HEmail = dto.Email,
                    HPassword = hash,
                    HSalt = salt,
                    HUserName = dto.UserName,
                    HPhoneNumber = dto.Phone,
                    HStatus = "已驗證",
                    HIsTenant = true,
                    HIsLandlord = false,
                    HCreatedAt = DateTime.UtcNow,
                    HUpdateAt = DateTime.UtcNow,
                    HIsDeleted = false,
                    HLoginFailCount = 0
                };

                _db.HTenants.Add(newTenant);
                _db.SaveChanges();

                return Ok(new { success = true, message = "註冊成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤",
                    error = ex.Message
                });
            }
        }

        // ✅ 雜湊驗證碼的方法
        private string HashToken(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
