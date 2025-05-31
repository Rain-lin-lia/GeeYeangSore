using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;

namespace GeeYeangSore.APIControllers.test
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecureTestController : BaseController
    {
        public SecureTestController(GeeYeangSoreContext db) : base(db) { }

        // 請依照你功能的需求選擇範本使用：

        // 會員（房客）專用功能範本
        [HttpGet("tenant-feature")]
        public IActionResult TenantFeature()
        {
            var access = CheckAccess(); // 已包含登入 + 黑名單判斷
            if (access != null) return access;

            // TODO：這裡開始寫房客專用邏輯
            var tenant = GetCurrentTenant();
            return Ok(new { success = true, message = $"房客 {tenant.HUserName} 操作成功" });
        }

        //  進階會員（房東）專用功能範本
        [HttpGet("landlord-feature")]
        public IActionResult LandlordFeature()
        {
            var access = CheckAccess(requireLandlord: true); // 含登入 + 黑名單 + 房東身份
            if (access != null) return access;

            // TODO：這裡開始寫房東專用邏輯
            var tenant = GetCurrentTenant();
            return Ok(new { success = true, message = $"房東 {tenant.HUserName} 功能執行成功" });
        }

    }
}
