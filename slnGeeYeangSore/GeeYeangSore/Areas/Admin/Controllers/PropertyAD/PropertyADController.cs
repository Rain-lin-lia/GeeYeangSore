using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
using GeeYeangSore.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GeeYeangSore.Areas.Admin.Controllers.PropertyAD
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class PropertyADController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public PropertyADController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string searchType = "all", string searchString = "", string sortOrder = "")
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            int pageSize = 15;

            // 設置排序參數
            ViewData["IdSort"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["TitleSort"] = sortOrder == "title" ? "title_desc" : "title";
            ViewData["LandlordSort"] = sortOrder == "landlord" ? "landlord_desc" : "landlord";
            ViewData["PropertySort"] = sortOrder == "property" ? "property_desc" : "property";
            ViewData["StatusSort"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["StartDateSort"] = sortOrder == "startdate" ? "startdate_desc" : "startdate";
            ViewData["EndDateSort"] = sortOrder == "enddate" ? "enddate_desc" : "enddate";
            ViewData["CurrentSort"] = sortOrder;

            var ads = _context.HAds
                .Include(a => a.HProperty)
                .Include(a => a.HLandlord)
                .Where(a => a.HIsDelete == null || a.HIsDelete == false)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                switch (searchType?.ToLower())
                {
                    case "id":
                        if (int.TryParse(searchString, out int adId))
                        {
                            ads = ads.Where(a => a.HAdId == adId);
                        }
                        break;
                    case "title":
                        ads = ads.Where(a => a.HAdName.Contains(searchString));
                        break;
                    case "landlord":
                        ads = ads.Where(a => a.HLandlord.HLandlordName.Contains(searchString));
                        break;
                    case "property":
                        ads = ads.Where(a => a.HProperty.HPropertyTitle.Contains(searchString));
                        break;
                    case "category":
                        ads = ads.Where(a => a.HCategory.Contains(searchString));
                        break;
                    case "status":
                        ads = ads.Where(a => a.HStatus.Contains(searchString));
                        break;
                    case "region":
                        ads = ads.Where(a => a.HTargetRegion.Contains(searchString));
                        break;
                    default: // "all" or any other value
                        ads = ads.Where(a =>
                            a.HAdId.ToString().Contains(searchString) ||
                            a.HAdName.Contains(searchString) ||
                            a.HCategory.Contains(searchString) ||
                            a.HStatus.Contains(searchString) ||
                            a.HTargetRegion.Contains(searchString) ||
                            a.HProperty.HPropertyTitle.Contains(searchString) ||
                            a.HLandlord.HLandlordName.Contains(searchString));
                        break;
                }
            }

            // 根據排序參數進行排序
            switch (sortOrder)
            {
                case "id_desc":
                    ads = ads.OrderByDescending(a => a.HAdId);
                    break;
                case "id":
                    ads = ads.OrderBy(a => a.HAdId);
                    break;
                case "title_desc":
                    ads = ads.OrderByDescending(a => a.HAdName);
                    break;
                case "title":
                    ads = ads.OrderBy(a => a.HAdName);
                    break;
                case "landlord_desc":
                    ads = ads.OrderByDescending(a => a.HLandlord.HLandlordName);
                    break;
                case "landlord":
                    ads = ads.OrderBy(a => a.HLandlord.HLandlordName);
                    break;
                case "property_desc":
                    ads = ads.OrderByDescending(a => a.HProperty.HPropertyTitle);
                    break;
                case "property":
                    ads = ads.OrderBy(a => a.HProperty.HPropertyTitle);
                    break;
                case "status_desc":
                    ads = ads.OrderByDescending(a => a.HStatus);
                    break;
                case "status":
                    ads = ads.OrderBy(a => a.HStatus);
                    break;
                case "startdate_desc":
                    ads = ads.OrderByDescending(a => a.HStartDate);
                    break;
                case "startdate":
                    ads = ads.OrderBy(a => a.HStartDate);
                    break;
                case "enddate_desc":
                    ads = ads.OrderByDescending(a => a.HEndDate);
                    break;
                case "enddate":
                    ads = ads.OrderBy(a => a.HEndDate);
                    break;
                default:
                    ads = ads.OrderBy(a => a.HAdId);
                    break;
            }

            // 確保在分頁前完成排序
            var result = await ads.ToListAsync();
            return View(result.ToPagedList(page, pageSize));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            if (id == null)
            {
                return NotFound();
            }

            var ad = await _context.HAds
                .Include(a => a.HProperty)
                .Include(a => a.HLandlord)
                .FirstOrDefaultAsync(m => m.HAdId == id);

            if (ad == null)
            {
                return NotFound();
            }

            return View(ad);
        }

        public IActionResult Create()
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            ViewData["HPropertyId"] = new SelectList(_context.HProperties.Where(p => p.HStatus == "已驗證"), "HPropertyId", "HPropertyTitle");
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords.Where(l => l.HStatus == "已驗證"), "HLandlordId", "HLandlordName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HAd ad)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                // 設置下拉選單資料
                SetViewData(ad);

                // 設置預設值
                ad.HCreatedDate = DateTime.Now;
                ad.HLastUpdated = DateTime.Now;
                ad.HIsDelete = false;
                
                // 添加記錄
                _context.HAds.Add(ad);
                var result = await _context.SaveChangesAsync();
                
                if (result > 0)
                {
                    TempData["SuccessMessage"] = "廣告已成功新增！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "無法保存廣告資料");
                    return View(ad);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Create action: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                ModelState.AddModelError("", $"新增廣告時發生錯誤：{ex.Message}");
                SetViewData(ad);
                return View(ad);
            }
        }

        private void SetViewData(HAd ad)
        {
            ViewData["HPropertyId"] = new SelectList(_context.HProperties.Where(p => p.HStatus == "已驗證"), "HPropertyId", "HPropertyTitle", ad?.HPropertyId);
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords.Where(l => l.HStatus == "已驗證"), "HLandlordId", "HLandlordName", ad?.HLandlordId);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            if (id == null)
            {
                return NotFound();
            }

            var ad = await _context.HAds.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            ViewData["HPropertyId"] = new SelectList(_context.HProperties.Where(p => p.HStatus == "已驗證"), "HPropertyId", "HPropertyTitle", ad.HPropertyId);
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords.Where(l => l.HStatus == "已驗證"), "HLandlordId", "HLandlordName", ad.HLandlordId);
            return View(ad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HAd ad)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                ad.HLastUpdated = DateTime.Now;
                _context.Update(ad);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "廣告已成功更新！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Edit action: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"更新廣告時發生錯誤：{ex.Message}");
                SetViewData(ad);
                return View(ad);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            if (id == null)
            {
                return NotFound();
            }

            var ad = await _context.HAds
                .Include(a => a.HProperty)
                .Include(a => a.HLandlord)
                .FirstOrDefaultAsync(m => m.HAdId == id);

            if (ad == null)
            {
                return NotFound();
            }

            return View(ad);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                var ad = await _context.HAds.FindAsync(id);
                if (ad != null)
                {
                    ad.HIsDelete = true;
                    ad.HLastUpdated = DateTime.Now;
                    _context.Update(ad);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "廣告已成功刪除！";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteConfirmed action: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"刪除廣告時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool AdExists(int id)
        {
            return _context.HAds.Any(e => e.HAdId == id);
        }
    }
}
