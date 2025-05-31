using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GeeYeangSore.APIControllers.Landlord
{
    [ApiController]
    [Route("api/[controller]")]
    public class BecomeLandlordController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public BecomeLandlordController(GeeYeangSoreContext context, IWebHostEnvironment environment) : base(context)
        {
            _environment = environment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] LandlordRegistrationModel model)
        {
            try
            {
                // 使用 BaseController 的 CheckAccess 方法進行權限檢查
                var accessCheck = CheckAccess();
                if (accessCheck != null)
                    return accessCheck;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                {
                    return Unauthorized(new { message = "未登入或找不到對應的租客帳號，請重新登入。" });
                }

                // 檢查是否已經是房東
                if (tenant.HIsLandlord)
                {
                    return BadRequest(new { message = "您已經是房東身份" });
                }

                var tenantId = tenant.HTenantId;

                // 處理圖片上傳
                string idCardFrontPath = "";
                string idCardBackPath = "";

                if (model.IdCardFront != null)
                    idCardFrontPath = await SaveImage(model.IdCardFront, "User");
                if (model.IdCardBack != null)
                    idCardBackPath = await SaveImage(model.IdCardBack, "User");

                // 新增房東資料
                var landlord = new HLandlord
                {
                    HTenantId = tenantId,
                    HLandlordName = model.Username,
                    HBankName = model.BankName,
                    HBankAccount = model.Bank,
                    HIdCardFrontUrl = idCardFrontPath,
                    HIdCardBackUrl = idCardBackPath,
                    HStatus = "驗證中",
                    HCreatedAt = DateTime.Now,
                    HUpdateAt = DateTime.Now,
                    HIsDeleted = false
                };

                _db.HLandlords.Add(landlord);
                await _db.SaveChangesAsync();

                return Ok(new { message = "註冊成功，等待審核" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task<string> SaveImage(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return "";

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.WebRootPath, "images", folder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }

    public class LandlordRegistrationModel
    {
        public string Username { get; set; }
        public string BankName { get; set; }
        public string Bank { get; set; }
        public IFormFile IdCardFront { get; set; }
        public IFormFile IdCardBack { get; set; }
    }
}
