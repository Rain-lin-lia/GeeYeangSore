using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.About;

[ApiController]
[Route("api/[controller]")]
public class AboutController : BaseController
{
    public AboutController(GeeYeangSoreContext db) : base(db) { }

    [HttpGet("about")]
    public IActionResult GetNews()
    {
        try
        {
            var about = _db.HAbouts.ToList();
            // // 驗證登入與權限
            // var access = CheckAccess();
            // if (access != null) return access;
            //
            // var tenant = GetCurrentTenant();
            // if (tenant == null)
            //     return Unauthorized(new { success = false, message = "未登入" });

            return Ok(new { response = about });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "處理請求時發生錯誤" });
        }
    }
}