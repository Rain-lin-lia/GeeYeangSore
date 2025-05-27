using GeeYeangSore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.DTO.Favorite;

namespace GeeYeangSore.APIControllers.Favorite
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : BaseController
    {
        public FavoriteController(GeeYeangSoreContext db) : base(db) { }

        [HttpGet("me")]
        public IActionResult GetMyFavorites()
        {
            try
            {

                var tenantId = HttpContext.Session.GetInt32("TenantId");
            if (tenantId == null)
                return Unauthorized(new { message = "尚未登入" });

            var list = _db.HFavorites
                .Where(f => f.HTenantId == tenantId)
                .Select(f => new
                {
                    propertyId = f.HProperty.HPropertyId,
                    title = f.HProperty.HPropertyTitle,
                    rentPrice = f.HProperty.HRentPrice,
                    city = f.HProperty.HCity,
                    district = f.HProperty.HDistrict,
                    address = f.HProperty.HAddress,
                    propertyType = f.HProperty.HPropertyType,
                    roomCount = f.HProperty.HRoomCount,
                    bathroomCount = f.HProperty.HBathroomCount,
                    image = f.HProperty.HPropertyImages
                        .Where(i => i.HIsDelete == false)
                        .OrderBy(i => i.HUploadedDate)
                        .Select(i => "https://localhost:7022" + i.HImageUrl)
                        .FirstOrDefault()
                })
                .ToList();

            return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "系統錯誤，請稍後再試。" });
            }
        }

        [HttpPost("add")]
        public IActionResult AddFavorite([FromBody] FavoriteDTO dto)
        {
            try     
            { 
            var tenantId = HttpContext.Session.GetInt32("TenantId");
            if (tenantId == null)
                return Unauthorized();

            var exists = _db.HFavorites.FirstOrDefault(f => f.HTenantId == tenantId && f.HPropertyId == dto.PropertyId);
            if (exists != null)
                return BadRequest("已收藏");

            _db.HFavorites.Add(new HFavorite
            {
                HTenantId = tenantId.Value,
                HPropertyId = dto.PropertyId,
                HCreatedAt = DateTime.Now
            });
            _db.SaveChanges();

            return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "系統錯誤，請稍後再試。" });
            }
        }

        [HttpPost("remove")]
        public IActionResult RemoveFavorite([FromBody] RemoveFavoriteDTO dto)
        {
            try
            { 
            var tenantId = HttpContext.Session.GetInt32("TenantId");
            if (tenantId == null)
                return Unauthorized();

            var favorite = _db.HFavorites.FirstOrDefault(f =>
                f.HTenantId == tenantId &&
                f.HPropertyId == dto.PropertyId
            );

            if (favorite == null)
                return NotFound();

            _db.HFavorites.Remove(favorite);
            _db.SaveChanges();

            return Ok(new { success = true, message = "已取消收藏" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "系統錯誤，請稍後再試。" });
            }
        }

        [HttpGet("list/{tenantId}")]
        public IActionResult GetFavorites(int tenantId)
        {
            try { 
            var list = _db.HFavorites
                .Where(f => f.HTenantId == tenantId)
                .Select(f => f.HPropertyId)
                .ToList();

            return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "系統錯誤，請稍後再試。" });
            }
        }
    }
}
