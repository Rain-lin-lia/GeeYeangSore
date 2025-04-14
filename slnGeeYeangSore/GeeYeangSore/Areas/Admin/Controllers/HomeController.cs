using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.ViewModels;
using System;
using System.Collections.Generic;
using GeeYeangSore.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Controllers;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : SuperController
    {
        private readonly GeeYeangSoreContext _context;

        public HomeController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            // 建立儀表板視圖模型並從資料庫填入數據
            var viewModel = new HomeDashboardViewModel
            {
                // 從資料庫抓取房源數（只計算已驗證的房源）
                PropertyCount = _context.HProperties
                    .Where(p => p.HStatus != "未驗證")
                    .Count(),

                // 從資料庫抓取新增用戶數（過去 30 天內註冊的用戶）
                // 參考 DataAnalysisController 的寫法
                NewUserCount = _context.HTenants
                    .Where(t => t.HCreatedAt.HasValue && t.HCreatedAt.Value >= thirtyDaysAgo)
                    .Count(),

                // 從資料庫抓取本月收入（參考 DataAnalysisController 的寫法）
                MonthlyIncome = _context.HTransactions
                    .Where(t => t.HPaymentDate.HasValue &&
                           t.HPaymentDate.Value.Month == currentMonth &&
                           t.HPaymentDate.Value.Year == currentYear &&
                           t.HTradeStatus == "Success")
                    .Sum(t => t.HAmount ?? 0),

                // 從資料庫抓取待審核房源數（參考 PropertyCheckController 的寫法）
                PendingPropertyCount = _context.HProperties
                    .Where(p => p.HStatus == "未驗證")
                    .Count(),

                // 從資料庫抓取待處理檢舉數
                PendingReportCount = _context.HReports
                    .Where(r => r.HStatus == "待處理")
                    .Count(),

                // 從資料庫抓取待審核房東數
                PendingLandlordCount = _context.HAudits
                    .Where(a => a.HStatus == "待審核")
                    .Count(),

                // 系統公告
                SystemAnnouncements = _context.HNews
                    .OrderByDescending(n => n.HCreatedAt)
                    .Take(3)
                    .Select(n => new SystemAnnouncement
                    {
                        Title = n.HTitle,
                        Content = n.HContent,
                        Date = n.HCreatedAt.ToString("yyyy/MM/dd")
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        public IActionResult NoPermission()
        {
            return View();
        }
    }
}
