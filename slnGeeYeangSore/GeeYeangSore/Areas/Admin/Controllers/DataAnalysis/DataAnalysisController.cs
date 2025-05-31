using GeeYeangSore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using static GeeYeangSore.Areas.Admin.ViewModels.DataAnalysisViewModel;
using GeeYeangSore.Areas.Admin.ViewModels;
using System.Linq;

namespace GeeYeangSore.Areas.Admin.Controllers.DataAnalysis
{
    // 數據分析控制器
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")]
    public class DataAnalysisController : SuperController
    {

        private readonly GeeYeangSoreContext _context;

        public DataAnalysisController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard(int? selectedYear)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
            {
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            }

            try
            {
                int year = selectedYear ?? DateTime.Now.Year;
                int month = DateTime.Now.Month;

                var dataAnalysis = new DataAnalysisViewModel
                {
                    // 每月新增物件
                    MonthlyPropertyData = _context.HProperties
                        .Where(p => p.HPublishedDate.HasValue)
                        .Where(p => p.HPublishedDate.Value.Year == year)
                        .GroupBy(p => p.HPublishedDate.Value.Month)
                        .Where(g => g.Key != null)
                        .Select(g => new { MonthKey = g.Key, PropertyCount = g.Count() })
                        .ToList()
                        .Select(g => new MonthlyPropertyData
                        {
                            Month = $"{year}-{g.MonthKey:D2}",
                            PropertyCount = g.PropertyCount
                        })
                        .OrderBy(m => m.Month)
                        .ToList(),

                    // 可選年份
                    SelectedYear = year,
                    PropertyAvailableYears = _context.HProperties
                                            .Where(p => p.HPublishedDate.HasValue)
                                            .Select(p => p.HPublishedDate.Value.Year)
                                            .Where(y => y > 2000)
                                            .Distinct()
                                            .OrderByDescending(y => y)
                                            .ToList(),

                    RevenueAvailableYears = _context.HTransactions
                                            .Where(t => t.HPaymentDate.HasValue)
                                            .Select(t => t.HPaymentDate.Value.Year)
                                            .Where(y => y > 2000)
                                            .Distinct()
                                            .OrderByDescending(y => y)
                                            .ToList()
                };

                // 當月統計
                dataAnalysis.CurrentMonthProperties = _context.HProperties.Count(p =>
                    p.HPublishedDate.HasValue &&
                    p.HPublishedDate.Value.Year == year &&
                    p.HPublishedDate.Value.Month == month &&
                    p.HStatus == "已驗證");

                dataAnalysis.CurrentMonthVipAds = _context.HAds
                    .Where(ad =>
                        (ad.HCategory == "VIP1" || ad.HCategory == "VIP2" || ad.HCategory == "VIP3") &&
                         ad.HStartDate.HasValue &&
                         ad.HStartDate.Value.Year == year &&
                         ad.HStartDate.Value.Month == month &&
                         ad.HIsDelete != true &&
                         ad.HStatus == "進行中")
                    .Count();

                dataAnalysis.CurrentMonthRevenue = _context.HTransactions
                    .Where(t => t.HPaymentDate.HasValue &&
                    t.HPaymentDate.Value.Year == year &&
                    t.HPaymentDate.Value.Month == month &&
                    t.HTradeStatus == "Success")
                    .Sum(t => t.HAmount ?? 0);

                dataAnalysis.CurrentMonthUsers = _context.HLandlords
                    .Where(l => l.HCreatedAt.Year == year &&
                                l.HCreatedAt.Month == month)
                    .Count() +
                    _context.HTenants
                    .Where(t => t.HCreatedAt.Year == year &&
                                t.HCreatedAt.Month == month)
                    .Count();

                // 總數據
                dataAnalysis.TotalUsers = _context.HLandlords.Count() + _context.HTenants.Count();
                dataAnalysis.TotalRevenue = _context.HRevenueReports
                    .Where(r => r.HReportDate.HasValue && r.HReportDate.Value.Year == year)
                    .Sum(r => r.HTotalIncome ?? 0);

                dataAnalysis.TotalLandlords = _context.HLandlords.Count();
                dataAnalysis.TotalTenants = _context.HTenants.Count();

                // VIP廣告分布
                var vipCategoryGroups = _context.HAds
                    .Where(ad =>
                        (ad.HCategory == "VIP1" || ad.HCategory == "VIP2" || ad.HCategory == "VIP3") &&
                        ad.HIsDelete != true && ad.HStatus == "進行中" && ad.HCategory != null)
                    .GroupBy(ad => ad.HCategory)
                    .Where(g => g.Key != null)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToList();
                dataAnalysis.VipCategoryDistribution = vipCategoryGroups
                    .ToDictionary(g => g.Category!, g => g.Count);

                // 地區柱狀圖
                var cityGroups = _context.HProperties
                    .Where(p => p.HDistrict != null && !string.IsNullOrEmpty(p.HCity))
                    .GroupBy(p => p.HCity ?? "Unknown")
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .Select(g => new { City = g.Key, Count = g.Count() })
                    .ToList();
                dataAnalysis.PropertiesByCity = cityGroups.ToDictionary(g => g.City!, g => g.Count);

                // 收益折線圖
                var monthlyRevenueGroups = _context.HTransactions
                    .Where(t => t.HPaymentDate.HasValue &&
                                t.HPaymentDate.Value.Year == year &&
                                t.HTradeStatus == "Success")
                    .ToList()
                    .GroupBy(t => t.HPaymentDate!.Value.Month)
                    .Where(g => g.Key != null)
                    .Select(g => new MonthlyRevenueData
                    {
                        Month = $"{year}-{g.Key:D2}",
                        Revenue = g.Sum(t => t.HAmount ?? 0)
                    })
                    .OrderBy(m => m.Month)
                    .ToList();
                dataAnalysis.MonthlyRevenueData = monthlyRevenueGroups;

                // 房源類型圓環圖
                var typeGroups = _context.HProperties
                    .Where(p => !string.IsNullOrEmpty(p.HPropertyType))
                    .GroupBy(p => p.HPropertyType)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToList();
                dataAnalysis.PropertyTypeCounts = typeGroups.ToDictionary(g => g.Type!, g => g.Count);

                return View(dataAnalysis);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString(), "text/plain");
            }
        }
        [HttpGet]
        public IActionResult LoadPropertyChart(int year)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
            {
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            }

            try
            {
                var model = new DataAnalysisViewModel
                {
                    MonthlyPropertyData = _context.HProperties
                    .Where(p => p.HPublishedDate.HasValue && p.HPublishedDate.Value.Year == year)
                    .GroupBy(p => p.HPublishedDate.Value.Month)
                    .ToList()
                    .Select(g => new MonthlyPropertyData
                    {
                        Month = $"{year}-{g.Key:D2}",
                        PropertyCount = g.Count()
                    })
                    .OrderBy(m => m.Month)
                    .ToList(),

                    SelectedYear = year,

                    PropertyAvailableYears = _context.HProperties
                    .Where(p => p.HPublishedDate.HasValue)
                    .Select(p => p.HPublishedDate.Value.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList()
                };

                return PartialView("_MonthlyPropertyChart", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "載入房源圖表失敗");
            }
        }
        [HttpGet]
        public IActionResult LoadRevenueChart(int year)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
            {
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            }

            try
            {
                var revenueData = _context.HTransactions
                .Where(t => t.HPaymentDate.HasValue && t.HPaymentDate.Value.Year == year && t.HTradeStatus == "Success")
                .ToList()
                .GroupBy(t => t.HPaymentDate!.Value.Month)
                .Select(g => new MonthlyRevenueData
                {
                    Month = $"{year}-{g.Key:D2}",
                    Revenue = g.Sum(t => t.HAmount ?? 0)
                })
                .OrderBy(m => m.Month)
                .ToList();

                var viewModel = new DataAnalysisViewModel
                {
                    MonthlyRevenueData = revenueData,
                    SelectedYear = year,
                    RevenueAvailableYears = _context.HTransactions
                        .Where(t => t.HPaymentDate.HasValue && t.HTradeStatus == "Success")
                        .Select(t => t.HPaymentDate.Value.Year)
                        .Distinct()
                        .OrderByDescending(y => y)
                        .ToList()
                };

                return PartialView("_MonthlyRevenueChart", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "載入收益圖表失敗");
            }
        }

        public IActionResult TenantDemandTable(int? selectedYear)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
            {
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            }

            //所有的 HFavorites 記錄中 HCreatedAt 都是 null
            int year = selectedYear ?? (_context.HFavorites
                .Where(f => f.HCreatedAt.HasValue)
                .Select(f => f.HCreatedAt.Value.Year)
                .Any() ? _context.HFavorites
                    .Where(f => f.HCreatedAt.HasValue)
                    .Select(f => f.HCreatedAt.Value.Year)
                    .Max() : DateTime.Now.Year);

            var favoritePropertyIds = _context.HFavorites
                .Where(f => f.HCreatedAt.HasValue && f.HCreatedAt.Value.Year == year)
                .Select(f => f.HPropertyId)
                .Distinct()
                .ToList();

            var featureData = _context.HPropertyFeatures
                .Where(f => favoritePropertyIds.Contains(f.HPropertyId))
                .ToList();

            var featureDemand = new List<FeatureDemandItem>
            {
                new() { FeatureName = "可養狗", Count = featureData.Count(f => f.HAllowsDogs == true) },
                new() { FeatureName = "可養貓", Count = featureData.Count(f => f.HAllowsCats == true) },
                new() { FeatureName = "可開伙", Count = featureData.Count(f => f.HAllowsCooking == true) },
                new() { FeatureName = "有家具", Count = featureData.Count(f => f.HHasFurniture == true) },
                new() { FeatureName = "有網路", Count = featureData.Count(f => f.HInternet == true) },
                new() { FeatureName = "有冷氣", Count = featureData.Count(f => f.HAirConditioning == true) },
                new() { FeatureName = "合租", Count = featureData.Count(f => f.HSharedRental == true) },
                new() { FeatureName = "有電視", Count = featureData.Count(f => f.HTv == true) },
                new() { FeatureName = "有冰箱", Count = featureData.Count(f => f.HRefrigerator == true) },
                new() { FeatureName = "有洗衣機", Count = featureData.Count(f => f.HWashingMachine == true) },
                new() { FeatureName = "有床", Count = featureData.Count(f => f.HBed == true) },
                new() { FeatureName = "有熱水器", Count = featureData.Count(f => f.HWaterHeater == true) },
                new() { FeatureName = "天然瓦斯", Count = featureData.Count(f => f.HGasStove == true) },
                new() { FeatureName = "第四台", Count = featureData.Count(f => f.HCableTv == true) },
                new() { FeatureName = "飲水機", Count = featureData.Count(f => f.HWaterDispenser == true) },
                new() { FeatureName = "車位", Count = featureData.Count(f => f.HParking == true) },
                new() { FeatureName = "社會住宅", Count = featureData.Count(f => f.HSocialHousing == true) },
                new() { FeatureName = "短期租賃", Count = featureData.Count(f => f.HShortTermRent == true) },
                new() { FeatureName = "公家電費", Count = featureData.Count(f => f.HPublicElectricity == true) },
                new() { FeatureName = "公家水費", Count = featureData.Count(f => f.HPublicWatercharges == true) },
                new() { FeatureName = "房東同住", Count = featureData.Count(f => f.HLandlordShared == true) },
                new() { FeatureName = "陽台", Count = featureData.Count(f => f.HBalcony == true) },
                new() { FeatureName = "公設", Count = featureData.Count(f => f.HPublicEquipment == true) }
            };

            int favoritePropertyCount = favoritePropertyIds.Count;
            int totalFeatureCount = featureDemand.Sum(f => f.Count);

            var viewModel = new TenantDemandTableViewModel
            {
                FeatureDemands = featureDemand,
                TotalCount = totalFeatureCount,
                FavoritePropertyCount = favoritePropertyCount,
                TotalFeatureCount = totalFeatureCount,
                SelectedYear = year,
                AvailableYears = _context.HFavorites
                    .Where(f => f.HCreatedAt.HasValue)
                    .Select(f => f.HCreatedAt.Value.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList()
            };

            return View(viewModel);
        }


    }
}
