using System.Runtime.InteropServices.JavaScript;
using GeeYeangSore.Controllers;
using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.Areas.Admin.Controllers.News

{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class AboutController : SuperController
    {

        private readonly GeeYeangSoreContext _db;
        private readonly IWebHostEnvironment _env;

        public AboutController(GeeYeangSoreContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;

        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"Index 頁面載入失敗: {ex.Message}");
                return RedirectToAction("Error", "Home", new { area = "Admin" });
            }
        }

        //https://localhost:7022/Admin/About/About
        public IActionResult About()
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var news = _db.HAbouts.ToList();

                return View(news);
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"關於我們頁面載入失敗: {ex.Message}");
                TempData["Error"] = "載入資料時發生錯誤，請稍後再試。";
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        [HttpPost]
        public IActionResult About(string HTitle, string HContent, IFormFile image)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                HAbout news = new HAbout
                {
                    HTitle = HTitle,
                    HContent = HContent,
                    HCreatedAt = DateTime.Now,
                    HUpdatedAt = DateTime.Now
                };
                _db.HAbouts.Add(news);
                _db.SaveChanges();

                TempData["Success"] = "新增關於我們成功！";
                return RedirectToAction("About");
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"新增關於我們失敗: {ex.Message}");
                TempData["Error"] = "新增資料時發生錯誤，請稍後再試。";
                return RedirectToAction("About");
            }
        }
        [HttpPost]
        public IActionResult UpdateAbout(int HAboutId, string HTitle, string HContent)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                var about = _db.HAbouts.FirstOrDefault(item => item.HAboutId == HAboutId);

                if (about != null)
                {
                    about.HTitle = HTitle;
                    about.HContent = HContent;
                    about.HUpdatedAt = DateTime.Now;


                    _db.SaveChanges();
                    TempData["Success"] = "公告修改成功！";
                }
                else
                {
                    TempData["Error"] = "找不到要修改的公告。";
                }

                return RedirectToAction("About");
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"更新關於我們失敗: {ex.Message}");
                TempData["Error"] = "更新資料時發生錯誤，請稍後再試。";
                return RedirectToAction("About");
            }
        }
        [HttpPost]
        public IActionResult DeleteAbout(int HAboutId)
        {
            try
            {
                if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                    //如果沒有權限就會顯示NoPermission頁面
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
                var about = _db.HAbouts.FirstOrDefault(item => item.HAboutId == HAboutId);

                if (about != null)
                {
                    _db.Remove(about);
                    _db.SaveChanges();
                    TempData["Success"] = "公告刪除成功！";
                }
                else
                {
                    TempData["Error"] = "找不到要刪除的公告。";
                }

                return RedirectToAction("About");
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"刪除關於我們失敗: {ex.Message}");
                TempData["Error"] = "刪除資料時發生錯誤，請稍後再試。";
                return RedirectToAction("About");
            }
        }

    }

}
