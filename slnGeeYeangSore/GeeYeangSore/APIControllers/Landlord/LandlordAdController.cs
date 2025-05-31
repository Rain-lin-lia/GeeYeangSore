using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GeeYeangSore.APIControllers.Landlord
{
    [ApiController]
    [Route("api/landlord/ads")]
    public class LandlordAdController : BaseController
    {
        public LandlordAdController(GeeYeangSoreContext db) : base(db)
        {
        }

        // 取得目前登入房東的所有廣告
        [HttpGet]
        public IActionResult GetMyAds()
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            var tenant = GetCurrentTenant();
            var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
            if (landlord == null)
                return Unauthorized(new { success = false, message = "找不到房東資料" });

            var ads = _db.HAds
                .Include(a => a.HProperty)
                .Include(a => a.HProperty.HPropertyImages)
                .Where(a => a.HLandlordId == landlord.HLandlordId && a.HIsDelete == false && a.HProperty.HStatus == "已驗證")
                .Select(a => new
                {
                    id = a.HAdId,
                    propertyId = a.HProperty.HPropertyId,
                    propertyTitle = a.HProperty.HPropertyTitle,
                    plan = a.HCategory,
                    status = a.HStatus,
                    coverImage = a.HProperty.HPropertyImages
                        .Where(i => i.HIsDelete == false)
                        .OrderBy(i => i.HUploadedDate)
                        .Select(i => i.HImageUrl)
                        .FirstOrDefault() ?? "/images/Property/default.jpg",
                    startDate = a.HStartDate,   // 新增
                    endDate = a.HEndDate        // 新增
                })
                .ToList();

            return Ok(ads);
        }

        // 更新廣告方案
        [HttpPut("{id}/plan")]
        public async Task<IActionResult> UpdateAdPlan(int id, [FromBody] UpdateAdPlanRequest request)
        {
            // 使用 BaseController 的 CheckAccess 方法進行權限檢查，要求房東權限
            var accessCheck = CheckAccess(requireLandlord: true);
            if (accessCheck != null)
                return accessCheck;

            try
            {
                var tenant = GetCurrentTenant();
                var landlord = _db.HLandlords.FirstOrDefault(l => l.HTenantId == tenant.HTenantId && !l.HIsDeleted);
                if (landlord == null)
                    return Unauthorized(new { success = false, message = "找不到房東資料" });

                var ad = await _db.HAds
                    .FirstOrDefaultAsync(a => a.HAdId == id && a.HLandlordId == landlord.HLandlordId && a.HIsDelete == false);

                if (ad == null)
                    return NotFound(new { success = false, message = "找不到廣告" });

                // 更新廣告方案
                ad.HCategory = request.Plan;
                ad.HLastUpdated = DateTime.Now;

                await _db.SaveChangesAsync();
                return Ok(new { success = true, message = "廣告方案更新成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "更新廣告方案失敗：" + ex.Message });
            }
        }
    }

    public class UpdateAdPlanRequest
    {
        public string Plan { get; set; }
    }
}