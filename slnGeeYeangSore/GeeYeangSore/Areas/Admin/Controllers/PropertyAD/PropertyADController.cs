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

        public async Task<IActionResult> Index(int page = 1)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            int pageSize = 15;
            var ads = await _context.HAds
                .Include(a => a.HProperty)
                .Include(a => a.HLandlord)
                .Where(a => a.HIsDelete == null || a.HIsDelete == false)
                .OrderByDescending(a => a.HCreatedDate)
                .ToListAsync();

            return View(ads.ToPagedList(page, pageSize));
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

            if (ModelState.IsValid)
            {
                ad.HCreatedDate = DateTime.Now;
                ad.HLastUpdated = DateTime.Now;
                ad.HIsDelete = false;
                
                _context.Add(ad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["HPropertyId"] = new SelectList(_context.HProperties.Where(p => p.HStatus == "已驗證"), "HPropertyId", "HPropertyTitle", ad.HPropertyId);
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords.Where(l => l.HStatus == "已驗證"), "HLandlordId", "HLandlordName", ad.HLandlordId);
            return View(ad);
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

            if (id != ad.HAdId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ad.HLastUpdated = DateTime.Now;
                    _context.Update(ad);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdExists(ad.HAdId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["HPropertyId"] = new SelectList(_context.HProperties.Where(p => p.HStatus == "已驗證"), "HPropertyId", "HPropertyTitle", ad.HPropertyId);
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords.Where(l => l.HStatus == "已驗證"), "HLandlordId", "HLandlordName", ad.HLandlordId);
            return View(ad);
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

            var ad = await _context.HAds.FindAsync(id);
            if (ad != null)
            {
                ad.HIsDelete = true;
                ad.HLastUpdated = DateTime.Now;
                _context.Update(ad);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AdExists(int id)
        {
            return _context.HAds.Any(e => e.HAdId == id);
        }
    }
}
