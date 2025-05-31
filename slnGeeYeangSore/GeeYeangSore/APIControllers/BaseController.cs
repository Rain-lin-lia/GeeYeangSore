using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace GeeYeangSore.APIControllers
{
    // 供所有前台API繼承的共用BaseController
    public class BaseController : ControllerBase
    {
        protected readonly GeeYeangSoreContext _db;

        public BaseController(GeeYeangSoreContext db)
        {
            _db = db;
        }

        // 取得目前登入房客（依Session Email）
        protected HTenant? GetCurrentTenant()
        {
            var email = HttpContext.Session.GetString(CDictionary.SK_FRONT_LOGINED_USER);
            if (string.IsNullOrEmpty(email))
                return null;
            return _db.HTenants.FirstOrDefault(t => t.HEmail == email && !t.HIsDeleted);
        }

        // 是否為房東
        protected bool IsLandlord()
        {
            var tenant = GetCurrentTenant();
            return tenant != null && tenant.HIsLandlord;
        }

        // 是否為黑名單
        protected bool IsBlacklisted()
        {
            var tenant = GetCurrentTenant();
            if (tenant == null) return false;

            var now = DateTime.UtcNow;

            // 檢查房客黑名單
            bool inTenantBlacklist = _db.HMblacklists.Any(b =>
                b.HTenantId == tenant.HTenantId &&
                (b.HExpirationDate == null || b.HExpirationDate > now)
            );

            // 如果不是房東就只回傳房客黑名單結果
            if (!tenant.HIsLandlord) return inTenantBlacklist;

            // 是房東，需額外檢查房東黑名單
            var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
            if (landlord == null) return inTenantBlacklist;

            bool inLandlordBlacklist = _db.HLblacklists.Any(b =>
                b.HLandlordId == landlord.HLandlordId &&
                (b.HExpirationDate == null || b.HExpirationDate > now)
            );

            return inTenantBlacklist || inLandlordBlacklist;
        }

        // 是否已登入
        protected bool IsLoggedIn()
        {
            var email = HttpContext.Session.GetString(CDictionary.SK_FRONT_LOGINED_USER);
            return !string.IsNullOrEmpty(email);
        }

        // 權限檢查：可用於 API 開頭統一驗證
        protected IActionResult? CheckAccess(bool requireLandlord = false)
        {
            if (!IsLoggedIn())
                return Unauthorized(new { success = false, message = "未登入" });

            if (IsBlacklisted())
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "您已被停權" });

            if (requireLandlord && !IsLandlord())
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "此功能僅限房東使用" });

            return null;
        }
    }
}