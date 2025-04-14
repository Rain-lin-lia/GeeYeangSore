using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models; // ✅ 加入 Models 命名空間
using GeeYeangSore.Controllers; // ✅ 加上這行

namespace GeeYeangSore.Areas.Admin.Controllers.UserManagement
{
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")] // ✅ 補上路由方便維護
    public class AdminManagementController : SuperController
    {
        private readonly GeeYeangSoreContext _context; // ✅ 注入 DbContext

        public AdminManagementController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        // 🐥 管理者列表主頁（可依帳號搜尋）
        public IActionResult AdminManagement(string? searchAccount = null)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "系統管理員"))
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var admins = _context.HAdmins
                    .Where(a => string.IsNullOrEmpty(searchAccount) || a.HAccount.Contains(searchAccount))
                    .OrderBy(a => a.HAdminId)
                    .ToList();

                ViewBag.IsSuperAdmin = HasAnyRole("超級管理員");
                return View("~/Areas/Admin/Views/User/AdminManagement.cshtml", admins);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 載入管理者資料失敗：{ex.Message}");
                return StatusCode(500, "載入管理者資料失敗");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("~/Areas/Admin/Partials/UserManagement/_CreateAdminPartial.cshtml", new HAdmin());
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Create([FromBody] HAdmin newAdmin)
        {
            try
            {
                var currentAccount = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
                var currentAdmin = _context.HAdmins.FirstOrDefault(a => a.HAccount == currentAccount);

                if (currentAdmin?.HRoleLevel != "超級管理員")
                    return Forbid();

                if (string.IsNullOrWhiteSpace(newAdmin.HAccount) || string.IsNullOrWhiteSpace(newAdmin.HPassword))
                    return BadRequest("帳號或密碼不得為空");

                newAdmin.HCreatedAt = DateTime.Now;
                newAdmin.HUpdateAt = DateTime.Now;

                _context.HAdmins.Add(newAdmin);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 建立管理員失敗：{ex.Message}");
                return StatusCode(500, "建立管理員失敗，請稍後再試");
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                var admin = _context.HAdmins.Find(id);
                if (admin == null)
                    return NotFound();

                return PartialView("~/Areas/Admin/Partials/UserManagement/_EditAdminPartial.cshtml", admin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 載入編輯視窗失敗：{ex.Message}");
                return StatusCode(500, "載入失敗");
            }
        }

        [HttpPost]
        public IActionResult Edit([FromBody] HAdmin edited)
        {
            try
            {
                var admin = _context.HAdmins.Find(edited.HAdminId);
                if (admin == null)
                    return NotFound();

                admin.HAccount = edited.HAccount;

                if (!string.IsNullOrWhiteSpace(edited.HPassword))
                    admin.HPassword = edited.HPassword;

                admin.HRoleLevel = edited.HRoleLevel;
                admin.HUpdateAt = DateTime.Now;

                _context.SaveChanges();
                return Ok("success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 編輯失敗：{ex.Message}");
                return StatusCode(500, "更新資料失敗");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var currentAccount = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
                var currentAdmin = _context.HAdmins.FirstOrDefault(a => a.HAccount == currentAccount);

                if (currentAdmin?.HRoleLevel != "超級管理員")
                    return Forbid();

                var admin = _context.HAdmins.Find(id);
                if (admin == null)
                    return NotFound();

                if (admin.HRoleLevel == "超級管理員")
                    return BadRequest("不可刪除超級管理員");

                _context.HAdmins.Remove(admin);
                _context.SaveChanges();

                return Ok("deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 刪除失敗：{ex.Message}");
                return StatusCode(500, "刪除失敗，請稍後再試");
            }
        }
    }
}
