using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using GeeYeangSore.Areas.Admin.HPropertyDto;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HPropertiesController : Controller
    {
        private readonly GeeYeangSoreContext _context;

        public HPropertiesController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        // GET: Admin/HProperties
        public async Task<IActionResult> Index()
        {
            var result = from h in _context.HProperties
                         join l in _context.HLandlords on h.HLandlordId equals l.HLandlordId
                         select new HPropertyDto
                         {
                              HPropertyId = h.HPropertyId,
                              HPropertyTitle = h.HPropertyTitle,
                              HDescription = h.HDescription,
                              HAddress = h.HAddress,
                              HCity = h.HCity,
                              HDistrict = h.HDistrict,
                              HZipcode = h.HZipcode,
                              HRentPrice = h.HRentPrice,
                              HPropertyType = h.HPropertyType,
                              HRoomCount = h.HRoomCount,
                              HBathroomCount = h.HBathroomCount,
                              HArea = h.HArea,
                              HFloor = h.HFloor,
                              HTotalFloors = h.HTotalFloors,
                              HAvailabilityStatus = h.HAvailabilityStatus,
                              HBuildingType = h.HBuildingType,
                              HScore = h.HScore,
                              HPublishedDate = h.HPublishedDate,
                              HLastUpdated = h.HLastUpdated,
                              HIsVip = h.HIsVip,
                              HIsShared = h.HIsShared,
                              LandlordName = l.HLandlordName // 假設 Landlord 有 Name 屬性
                          };
            return View(await result.ToListAsync());
        }


        // GET: Admin/HProperties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hProperty = await _context.HProperties
                .Include(h => h.HLandlord)
                .FirstOrDefaultAsync(m => m.HPropertyId == id);
            if (hProperty == null)
            {
                return NotFound();
            }

            return View(hProperty);
        }

        // GET: Admin/HProperties/Create
        public IActionResult Create()
        {
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordId");
            return View();
        }

        // POST: Admin/HProperties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HPropertyId,HLandlordId,HPropertyTitle,HDescription,HAddress,HCity,HDistrict,HZipcode,HRentPrice,HPropertyType,HRoomCount,HBathroomCount,HArea,HFloor,HTotalFloors,HAvailabilityStatus,HBuildingType,HScore,HPublishedDate,HLastUpdated,HIsVip,HIsShared")] HProperty hProperty)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 設置默認值（如果需要）
                    if (hProperty.HPublishedDate == null)
                    {
                        hProperty.HPublishedDate = DateTime.Now;
                    }
                    if (hProperty.HLastUpdated == null)
                    {
                        hProperty.HLastUpdated = DateTime.Now;
                    }

                    _context.Add(hProperty);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // 記錄錯誤（在生產環境中可以使用日誌框架）
                    ModelState.AddModelError("", "保存失敗：" + ex.Message);
                }
            }

            // 如果 ModelState 無效，顯示驗證錯誤
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage); // 或者使用日誌
            }

            ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordId", hProperty.HLandlordId);
            return View(hProperty);
        }

        // GET: Admin/HProperties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hProperty = await _context.HProperties.FindAsync(id);
            if (hProperty == null)
            {
                return NotFound();
            }
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordId", hProperty.HLandlordId);
            return View(hProperty);
        }

        // POST: Admin/HProperties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HPropertyId,HLandlordId,HPropertyTitle,HDescription,HAddress,HCity,HDistrict,HZipcode,HRentPrice,HPropertyType,HRoomCount,HBathroomCount,HArea,HFloor,HTotalFloors,HAvailabilityStatus,HBuildingType,HScore,HPublishedDate,HLastUpdated,HIsVip,HIsShared")] HProperty hProperty)
        {
            if (id != hProperty.HPropertyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hProperty);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HPropertyExists(hProperty.HPropertyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordId", hProperty.HLandlordId);
            return View(hProperty);
        }

        // GET: Admin/HProperties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hProperty = await _context.HProperties
                .Include(h => h.HLandlord)
                .FirstOrDefaultAsync(m => m.HPropertyId == id);
            if (hProperty == null)
            {
                return NotFound();
            }

            return View(hProperty);
        }

        // POST: Admin/HProperties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hProperty = await _context.HProperties.FindAsync(id);
            if (hProperty != null)
            {
                _context.HProperties.Remove(hProperty);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HPropertyExists(int id)
        {
            return _context.HProperties.Any(e => e.HPropertyId == id);
        }
    }
}