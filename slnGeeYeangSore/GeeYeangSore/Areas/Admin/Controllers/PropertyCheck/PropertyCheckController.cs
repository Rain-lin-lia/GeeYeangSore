using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
using System.IO;
using GeeYeangSore.Controllers;
using System;

namespace GeeYeangSore.Areas.Admin.Controllers.PropertyCheck
{
    // GET:https://localhost:7022/Admin/PropertyCheck/Index
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]

    public class PropertyCheckController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public PropertyCheckController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(int page = 1, string searchType = "all", string searchString = "", string sortOrder = "")
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            int pageSize = 15; // 每頁15筆資料

            // 設置排序參數
            ViewData["IdSort"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["TitleSort"] = sortOrder == "title" ? "title_desc" : "title";
            ViewData["LandlordSort"] = sortOrder == "landlord" ? "landlord_desc" : "landlord";
            ViewData["AddressSort"] = sortOrder == "address" ? "address_desc" : "address";
            ViewData["PriceSort"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["DateSort"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["CurrentSort"] = sortOrder;

            var query = _context.HProperties
                .Include(p => p.HLandlord)
                .Where(p => p.HStatus == "未驗證" && (p.HIsDelete == null || p.HIsDelete == false));

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                switch (searchType?.ToLower())
                {
                    case "id":
                        if (int.TryParse(searchString, out int propertyId))
                        {
                            query = query.Where(p => p.HPropertyId == propertyId);
                        }
                        break;
                    case "title":
                        query = query.Where(p => p.HPropertyTitle.Contains(searchString));
                        break;
                    case "address":
                        query = query.Where(p => p.HAddress.Contains(searchString));
                        break;
                    case "city":
                        query = query.Where(p => p.HCity.Contains(searchString));
                        break;
                    case "district":
                        query = query.Where(p => p.HDistrict.Contains(searchString));
                        break;
                    case "landlord":
                        query = query.Where(p => p.HLandlord.HLandlordName.Contains(searchString));
                        break;
                    case "type":
                        query = query.Where(p => p.HPropertyType.Contains(searchString));
                        break;
                    case "price":
                        if (int.TryParse(searchString, out int rentPrice))
                        {
                            query = query.Where(p => p.HRentPrice == rentPrice);
                        }
                        break;
                    default: // "all" or any other value
                        query = query.Where(p =>
                            p.HPropertyId.ToString().Contains(searchString) ||
                            p.HPropertyTitle.Contains(searchString) ||
                            p.HAddress.Contains(searchString) ||
                            p.HCity.Contains(searchString) ||
                            p.HDistrict.Contains(searchString) ||
                            p.HPropertyType.Contains(searchString) ||
                            p.HRentPrice.ToString().Contains(searchString) ||
                            p.HLandlord.HLandlordName.Contains(searchString));
                        break;
                }
            }

            // 根據排序參數進行排序
            switch (sortOrder)
            {
                case "id_desc":
                    query = query.OrderByDescending(p => p.HPropertyId);
                    break;
                case "id":
                    query = query.OrderBy(p => p.HPropertyId);
                    break;
                case "title_desc":
                    query = query.OrderByDescending(p => p.HPropertyTitle);
                    break;
                case "title":
                    query = query.OrderBy(p => p.HPropertyTitle);
                    break;
                case "landlord_desc":
                    query = query.OrderByDescending(p => p.HLandlord.HLandlordName);
                    break;
                case "landlord":
                    query = query.OrderBy(p => p.HLandlord.HLandlordName);
                    break;
                case "address_desc":
                    query = query.OrderByDescending(p => p.HAddress);
                    break;
                case "address":
                    query = query.OrderBy(p => p.HAddress);
                    break;
                case "price":
                    query = query.OrderBy(p => p.HRentPrice);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.HRentPrice);
                    break;
                case "date":
                    query = query.OrderBy(p => p.HPublishedDate);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(p => p.HPublishedDate);
                    break;
                default:
                    query = query.OrderBy(p => p.HPropertyId);
                    break;
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["SearchType"] = searchType;

            var properties = await query.ToListAsync();
            return View(properties.ToPagedList(page, pageSize));
        }

        // GET: Admin/PropertyCheck/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.HProperties
                .Include(p => p.HLandlord)
                .Include(p => p.HPropertyFeatures)
                .Include(p => p.HPropertyImages)
                .FirstOrDefaultAsync(m => m.HPropertyId == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: Admin/PropertyCheck/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            var property = await _context.HProperties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            property.HStatus = "已驗證";
            _context.Update(property);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/PropertyCheck/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // 開始交易
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 獲取房源及其相關資料
                        var property = await _context.HProperties
                            .Include(p => p.HPropertyImages)
                            .Include(p => p.HPropertyFeatures)
                            .Include(p => p.HPropertyAudits)
                            .FirstOrDefaultAsync(p => p.HPropertyId == id);

                        if (property == null)
                        {
                            return NotFound();
                        }

                        // 1. 軟刪除房源圖片
                        if (property.HPropertyImages != null && property.HPropertyImages.Any())
                        {
                            foreach (var image in property.HPropertyImages)
                            {
                                image.HIsDelete = true;
                            }
                            await _context.SaveChangesAsync();
                        }

                        // 2. 軟刪除房源特色
                        if (property.HPropertyFeatures != null && property.HPropertyFeatures.Any())
                        {
                            foreach (var feature in property.HPropertyFeatures)
                            {
                                feature.HIsDelete = true;
                            }
                            await _context.SaveChangesAsync();
                        }

                        // 3. 軟刪除房源審核記錄
                        if (property.HPropertyAudits != null && property.HPropertyAudits.Any())
                        {
                            foreach (var audit in property.HPropertyAudits)
                            {
                                audit.HIsDelete = true;
                            }
                            await _context.SaveChangesAsync();
                        }

                        // 4. 最後軟刪除房源本身
                        property.HIsDelete = true;
                        await _context.SaveChangesAsync();

                        // 提交交易
                        await transaction.CommitAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        // 回滾交易
                        await transaction.RollbackAsync();
                        return View("Error");
                    }
                }
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
    }
}
