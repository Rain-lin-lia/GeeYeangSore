using GeeYeangSore.Areas.Admin.ViewModels.UserManagement;
using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace GeeYeangSore.Areas.Admin.Controllers.UserManagement
{
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")]
    public class UserController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public UserController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        // 初始頁面：顯示所有使用者
        public IActionResult UserManagement(int page = 1)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "客服管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            int pageSize = 15;
            var allUsers = _context.HTenants
                .Where(t => !t.HIsDeleted) // 過濾未被刪除的使用者
                .Include(t => t.HLandlords)
                .AsEnumerable()
                .Select(t =>
                {
                    var landlord = t.HLandlords.FirstOrDefault();
                    return new CUserViewModels
                    {
                        TenantId = t.HTenantId,
                        TenantStatus = t.HStatus?.Trim() ?? "未設定",
                        LandlordId = landlord?.HLandlordId.ToString() ?? "-",
                        LandlordStatus = landlord?.HStatus?.Trim() ?? "未驗證",
                        Name = t.HUserName ?? "未填寫",
                        RegisterDate = t.HCreatedAt,
                        IsTenant = t.HIsTenant,
                        IsLandlord = t.HIsLandlord
                    };
                })
                .ToPagedList(page, pageSize);

            return View(allUsers);
        }

        [HttpPost]
        public IActionResult SearchUser([FromBody] CUserSearchViewModel query, int page = 1)
        {
            if (query == null)
            {
                return BadRequest("查詢條件為空");
            }

            int pageSize = 15;

            var result = _context.HTenants
                .Where(t => !t.HIsDeleted) // 過濾未被刪除的使用者
                .Include(t => t.HLandlords)
                .AsEnumerable()
                .Select(t =>
                {
                    var landlord = t.HLandlords.FirstOrDefault();
                    return new CUserViewModels
                    {
                        TenantId = t.HTenantId,
                        TenantStatus = t.HStatus?.Trim() ?? "未設定",
                        LandlordId = landlord?.HLandlordId.ToString() ?? "-",
                        LandlordStatus = landlord?.HStatus?.Trim() ?? "未驗證",
                        Name = t.HUserName ?? "未填寫",
                        RegisterDate = t.HCreatedAt,
                        IsTenant = t.HIsTenant,
                        IsLandlord = t.HIsLandlord
                    };
                })
                .Where(u =>
                    (string.IsNullOrEmpty(query.UserId) || u.TenantId.ToString().Contains(query.UserId) || u.LandlordId.Contains(query.UserId)) &&
                    (string.IsNullOrEmpty(query.Name) || u.Name.Contains(query.Name)) &&
                    (string.IsNullOrEmpty(query.Status) || u.TenantStatus == query.Status || u.LandlordStatus == query.Status) &&
                    (!query.StartDate.HasValue || u.RegisterDate >= query.StartDate.Value) &&
                    (!query.EndDate.HasValue || u.RegisterDate <= query.EndDate.Value) &&
                    (!query.IsLandlord.HasValue || u.IsLandlord == query.IsLandlord.Value)
                )
                .ToPagedList(page, pageSize);

            return PartialView("~/Areas/Admin/Partials/UserManagement/_UserListPartial.cshtml", result);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var tenant = _context.HTenants
                .Include(t => t.HLandlords)
                    .ThenInclude(l => l.HProperties)
                .FirstOrDefault(t => t.HTenantId == id);

            if (tenant == null)
                return NotFound();

            return PartialView("~/Areas/Admin/Partials/UserManagement/_EditUserPartial.cshtml", tenant);
        }

        [HttpPut]
        public IActionResult Update([FromBody] CEditUserViewModel updated)
        {
            if (updated == null)
                return BadRequest("未填寫資料");

            try
            {
                var existing = _context.HTenants
                    .Include(t => t.HLandlords)
                    .FirstOrDefault(t => t.HTenantId == updated.HTenantId && !t.HIsDeleted);
                if (existing == null)
                    return NotFound("找不到此使用者");

                // 檢查必填欄位
                if (string.IsNullOrEmpty(updated.HUserName) || 
                    string.IsNullOrEmpty(updated.HPhoneNumber) || 
                    string.IsNullOrEmpty(updated.HEmail))
                {
                    return BadRequest("姓名、電話和電子信箱為必填欄位");
                }

                // 檢查電話是否被其他使用者使用
                if (!string.IsNullOrEmpty(updated.HPhoneNumber) && 
                    existing.HPhoneNumber != updated.HPhoneNumber && 
                    _context.HTenants.Any(t => t.HPhoneNumber == updated.HPhoneNumber && t.HTenantId != updated.HTenantId && !t.HIsDeleted))
                {
                    return BadRequest("電話號碼已被其他使用者使用，請使用其他電話號碼");
                }

                // 檢查電子郵件是否被其他使用者使用
                if (!string.IsNullOrEmpty(updated.HEmail) && 
                    existing.HEmail != updated.HEmail && 
                    _context.HTenants.Any(t => t.HEmail == updated.HEmail && t.HTenantId != updated.HTenantId && !t.HIsDeleted))
                {
                    return BadRequest("電子郵件已被其他使用者使用，請使用其他電子郵件");
                }

                existing.HUserName = updated.HUserName;
                existing.HStatus = updated.HStatus ?? existing.HStatus;
                existing.HBirthday = updated.HBirthday;  // 可選欄位，直接賦值
                existing.HGender = updated.HGender;      // 可選欄位，直接賦值
                existing.HAddress = updated.HAddress;    // 可選欄位，直接賦值
                existing.HPhoneNumber = updated.HPhoneNumber;
                existing.HEmail = updated.HEmail;
                existing.HUpdateAt = DateTime.Now;       // 更新修改時間

                // 如果密碼有更新，則重新生成鹽值與哈希密碼
                if (!string.IsNullOrEmpty(updated.HPassword))
                {
                    string salt = PasswordHasher.GenerateSalt();
                    existing.HPassword = PasswordHasher.HashPassword(updated.HPassword, salt);
                }

                existing.HImages = updated.HImages;  // 可選欄位，直接賦值

                var updatedLandlord = updated.HLandlords.FirstOrDefault();
                var existingLandlord = existing.HLandlords.FirstOrDefault();

                if (updatedLandlord != null && existingLandlord != null)
                {
                    Console.WriteLine("🪪 房東資料更新內容：");
                    Console.WriteLine($"▶️ 房東本名：{existingLandlord.HLandlordName} → {updatedLandlord.HLandlordName}");
                    Console.WriteLine($"▶️ 身份狀態：{existingLandlord.HStatus} → {updatedLandlord.HStatus}");
                    Console.WriteLine($"▶️ 銀行名稱：{existingLandlord.HBankName} → {updatedLandlord.HBankName}");
                    Console.WriteLine($"▶️ 銀行帳戶：{existingLandlord.HBankAccount} → {updatedLandlord.HBankAccount}");
                    Console.WriteLine($"▶️ 正面：{existingLandlord.HIdCardFrontUrl} → {updatedLandlord.HIdCardFrontUrl}");
                    Console.WriteLine($"▶️ 反面：{existingLandlord.HIdCardBackUrl} → {updatedLandlord.HIdCardBackUrl}");

                    // 直接更新房東資料
                    existingLandlord.HLandlordName = updatedLandlord.HLandlordName ?? "";
                    existingLandlord.HStatus = updatedLandlord.HStatus ?? "";
                    existingLandlord.HBankName = updatedLandlord.HBankName ?? "";
                    existingLandlord.HBankAccount = updatedLandlord.HBankAccount ?? "";
                    existingLandlord.HIdCardFrontUrl = updatedLandlord.HIdCardFrontUrl ?? "";
                    existingLandlord.HIdCardBackUrl = updatedLandlord.HIdCardBackUrl ?? "";
                    existingLandlord.HUpdateAt = DateTime.Now;  // 更新修改時間

                    // 根據房東狀態更新 HTenant 的 HIsLandlord 欄位
                    existing.HIsLandlord = updatedLandlord.HStatus == "已驗證";

                    _context.HLandlords.Update(existingLandlord);
                }
                else
                {
                    Console.WriteLine("⚠️ 房東資料為空，無法更新");
                }

                _context.HTenants.Update(existing);
                int affected = _context.SaveChanges();
                Console.WriteLine($"🟢 儲存成功，共更新 {affected} 筆資料");
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 儲存失敗，錯誤訊息：{ex.Message}");
                return StatusCode(500, "資料儲存失敗，請稍後再試");
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var tenant = _context.HTenants.FirstOrDefault(t => t.HTenantId == id);
                if (tenant != null)
                {
                    tenant.HIsDeleted = true; // 設為軟刪除
                    tenant.HUpdateAt = DateTime.Now; // 更新修改時間
                    _context.HTenants.Update(tenant);
                    _context.SaveChanges();
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 軟刪除失敗：{ex.Message}");
                return StatusCode(500, "刪除失敗，請稍後再試");
            }
        }


        [HttpPost]
        public IActionResult UploadTenantPhoto(IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                    return BadRequest("未選擇檔案");

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                var savePath = Path.Combine("wwwroot/images/User", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                return Ok("/images/User/" + fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 上傳照片失敗：{ex.Message}");
                return StatusCode(500, "上傳照片失敗");
            }
        }

        [HttpPost]
        public IActionResult UploadLandlordIdFront(IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                    return BadRequest("未選擇檔案");

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                var savePath = Path.Combine("wwwroot/images/User", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                return Ok("/images/User/" + fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 上傳正面失敗：{ex.Message}");
                return StatusCode(500, "上傳失敗");
            }
        }

        [HttpPost]
        public IActionResult UploadLandlordIdBack(IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                    return BadRequest("未選擇檔案");

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                var savePath = Path.Combine("wwwroot/images/User", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                return Ok("/images/User/" + fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 上傳反面失敗：{ex.Message}");
                return StatusCode(500, "上傳失敗");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("~/Areas/Admin/Partials/UserManagement/_CreateUserPartial.cshtml", new CEditUserViewModel());
        }

        [HttpPost]
        public IActionResult Create([FromBody] CEditUserViewModel newUser)
        {
            if (newUser == null)
                return BadRequest("未填寫資料");

            try
            {
                // 檢查必填欄位
                if (string.IsNullOrEmpty(newUser.HUserName) || 
                    string.IsNullOrEmpty(newUser.HPhoneNumber) || 
                    string.IsNullOrEmpty(newUser.HEmail) || 
                    string.IsNullOrEmpty(newUser.HPassword))
                {
                    return BadRequest("姓名、電話、電子信箱和密碼為必填欄位");
                }

                // 檢查電話是否重複
                if (!string.IsNullOrEmpty(newUser.HPhoneNumber) && 
                    _context.HTenants.Any(t => t.HPhoneNumber == newUser.HPhoneNumber && !t.HIsDeleted))
                {
                    return BadRequest("電話號碼已被使用，請使用其他電話號碼");
                }

                // 檢查電子郵件是否重複
                if (!string.IsNullOrEmpty(newUser.HEmail) && 
                    _context.HTenants.Any(t => t.HEmail == newUser.HEmail && !t.HIsDeleted))
                {
                    return BadRequest("電子郵件已被使用，請使用其他電子郵件");
                }

                // 生成鹽值與哈希密碼
                string salt = PasswordHasher.GenerateSalt();
                string hashedPassword = PasswordHasher.HashPassword(newUser.HPassword, salt);

                var tenant = new HTenant
                {
                    HUserName = newUser.HUserName,
                    HBirthday = newUser.HBirthday,
                    HGender = newUser.HGender,
                    HPhoneNumber = newUser.HPhoneNumber,
                    HEmail = newUser.HEmail,
                    HPassword = hashedPassword,
                    HSalt = salt,
                    HAddress = newUser.HAddress,
                    HStatus = newUser.HStatus ?? "未驗證",
                    HImages = newUser.HImages,
                    HCreatedAt = DateTime.Now,
                    HUpdateAt = DateTime.Now,
                    HIsTenant = true,
                    HIsLandlord = false,
                    HIsDeleted = false,
                    HLoginFailCount = 0
                };

                _context.HTenants.Add(tenant);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 建立房客失敗：{ex.Message}");
                return StatusCode(500, "建立失敗，請稍後再試");
            }
        }

        // 檢查電話號碼是否存在
        [HttpGet]
        public IActionResult CheckPhoneExists(string phoneNumber, int? excludeId = null)
        {
            bool exists = _context.HTenants
                .Any(t => t.HPhoneNumber == phoneNumber && 
                         !t.HIsDeleted && 
                         (!excludeId.HasValue || t.HTenantId != excludeId.Value));
                         
            return Json(new { exists });
        }

        // 檢查電子郵件是否存在
        [HttpGet]
        public IActionResult CheckEmailExists(string email, int? excludeId = null)
        {
            bool exists = _context.HTenants
                .Any(t => t.HEmail == email && 
                         !t.HIsDeleted && 
                         (!excludeId.HasValue || t.HTenantId != excludeId.Value));
                         
            return Json(new { exists });
        }

    }
}
