using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;

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
            var geeYeangSoreContext = _context.HProperties.Include(h => h.HLandlord);
            return View(await geeYeangSoreContext.ToListAsync());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HPropertyId,HLandlordId,HPropertyTitle,HDescription,HAddress,HCity,HDistrict,HZipcode,HRentPrice,HPropertyType,HRoomCount,HBathroomCount,HArea,HFloor,HTotalFloors,HAvailabilityStatus,HBuildingType,HScore,HPublishedDate,HLastUpdated,HIsVip,HIsShared")] HProperty hProperty)
        {
            
            
                _context.Add(hProperty);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
