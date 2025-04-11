using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using GeeYeangSore.Controllers;

namespace GeeYeangSore.Areas.Admin.Controllers.Property
{
    /// <summary>
    /// 房源管理控制器
    /// </summary>
    /// // GET:https://localhost:7022/Admin/Property/Index
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class PropertyController : SuperController
    {
        // 資料庫上下文
        private readonly GeeYeangSoreContext _context;
        private readonly ILogger<PropertyController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // 建構函式，注入資料庫上下文
        public PropertyController(GeeYeangSoreContext context, ILogger<PropertyController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        
        /// <summary>
        /// 房源列表頁面，支援搜尋功能
        /// </summary>
        /// <param name="searchString">搜尋關鍵字</param>
        /// <param name="searchType">搜尋類型</param>
        /// <param name="sortOrder">排序參數</param>
        /// <param name="page">當前頁碼</param>
        /// <returns>房源列表視圖</returns>
        public async Task<IActionResult> Index(string searchString, string searchType, string sortOrder, int page = 1)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            try
            {
                int pageSize = 15; // 每頁15筆資料

                // 設置排序參數
                ViewData["IdSort"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
                ViewData["TitleSort"] = sortOrder == "title" ? "title_desc" : "title";
                ViewData["LandlordSort"] = sortOrder == "landlord" ? "landlord_desc" : "landlord";
                ViewData["AddressSort"] = sortOrder == "address" ? "address_desc" : "address";
                ViewData["PriceSort"] = sortOrder == "price" ? "price_desc" : "price";
                ViewData["DateSort"] = sortOrder == "date" ? "date_desc" : "date";
                ViewData["CurrentSort"] = sortOrder;

                var properties = _context.HProperties
                    .Include(p => p.HLandlord)
                    .Where(p => p.HStatus == "已驗證" && p.HLandlord.HStatus == "已驗證")  // 同時篩選已驗證的房源和房東
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim();
                    switch (searchType?.ToLower())
                    {
                        case "id":
                            if (int.TryParse(searchString, out int propertyId))
                            {
                                properties = properties.Where(p => p.HPropertyId == propertyId);
                            }
                            break;
                        case "title":
                            properties = properties.Where(p => p.HPropertyTitle.Contains(searchString));
                            break;
                        case "address":
                            properties = properties.Where(p => p.HAddress.Contains(searchString));
                            break;
                        case "city":
                            properties = properties.Where(p => p.HCity.Contains(searchString));
                            break;
                        case "district":
                            properties = properties.Where(p => p.HDistrict.Contains(searchString));
                            break;
                        case "landlord":
                            properties = properties.Where(p => p.HLandlord.HLandlordName.Contains(searchString));
                            break;
                        case "type":
                            properties = properties.Where(p => p.HPropertyType.Contains(searchString));
                            break;
                        case "rent":
                            if (int.TryParse(searchString, out int rentPrice))
                            {
                                properties = properties.Where(p => p.HRentPrice == rentPrice);
                            }
                            break;
                        default: // "all" or any other value
                            properties = properties.Where(p =>
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
                        properties = properties.OrderByDescending(p => p.HPropertyId);
                        break;
                    case "id":
                        properties = properties.OrderBy(p => p.HPropertyId);
                        break;
                    case "title_desc":
                        properties = properties.OrderByDescending(p => p.HPropertyTitle);
                        break;
                    case "title":
                        properties = properties.OrderBy(p => p.HPropertyTitle);
                        break;
                    case "landlord_desc":
                        properties = properties.OrderByDescending(p => p.HLandlord.HLandlordName);
                        break;
                    case "landlord":
                        properties = properties.OrderBy(p => p.HLandlord.HLandlordName);
                        break;
                    case "address_desc":
                        properties = properties.OrderByDescending(p => p.HAddress);
                        break;
                    case "address":
                        properties = properties.OrderBy(p => p.HAddress);
                        break;
                    case "price":
                        properties = properties.OrderBy(p => p.HRentPrice);
                        break;
                    case "price_desc":
                        properties = properties.OrderByDescending(p => p.HRentPrice);
                        break;
                    case "date":
                        properties = properties.OrderBy(p => p.HPublishedDate);
                        break;
                    case "date_desc":
                        properties = properties.OrderByDescending(p => p.HPublishedDate);
                        break;
                    default: // "id" or empty
                        properties = properties.OrderBy(p => p.HPropertyId);
                        break;
                }

                ViewData["CurrentFilter"] = searchString;
                ViewData["SearchType"] = searchType;

                var result = await properties.ToListAsync();
                return View(result.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving properties");
                return View("Error");
            }
        }

        /// <summary>
        /// 顯示房源詳細資訊
        /// </summary>
        /// <param name="id">房源ID</param>
        /// <returns>房源詳細資訊視圖</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var property = await _context.HProperties
                    .Include(p => p.HLandlord)
                    .Include(p => p.HPropertyFeatures)  // 加入房源特色
                    .Include(p => p.HPropertyImages)    // 加入房源照片
                    .Where(p => p.HStatus == "已驗證" && p.HLandlord.HStatus == "已驗證")
                    .FirstOrDefaultAsync(m => m.HPropertyId == id);
                
                if (property == null)
                {
                    return NotFound();
                }

                return View(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving property details");
                return View("Error");
            }
        }

        /// <summary>
        /// 顯示新增房源頁面
        /// </summary>
        
        public IActionResult Create()
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            try
            {
                ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing create view");
                return View("Error");
            }
        }

        /// <summary>
        /// 處理新增房源的請求
        /// </summary>
        /// <param name="property">房源資料</param>
        /// <param name="features">房源特色</param>
        /// <param name="images">房源照片</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HProperty property, HPropertyFeature features, List<IFormFile> images)
        {
            try
            {
                // 設置創建時間和更新時間
                property.HPublishedDate = DateTime.Now;
                property.HLastUpdated = DateTime.Now;
                property.HStatus = "已驗證"; // 設置初始狀態為待審核
                
                // 設置其他預設值
                property.HScore = string.IsNullOrEmpty(property.HScore) ? "0" : property.HScore;
                property.HIsVip = property.HIsVip ?? false;
                property.HIsShared = property.HIsShared ?? false;
                property.HAvailabilityStatus = string.IsNullOrEmpty(property.HAvailabilityStatus) ? "可租" : property.HAvailabilityStatus;

                // 修復 features 的 Checkbox 值
                features.HAllowsAnimals = Request.Form["features.HAllowsAnimals"].ToString() == "on";
                features.HAllowsDogs = Request.Form["features.HAllowsDogs"].ToString() == "on";
                features.HAllowsCats = Request.Form["features.HAllowsCats"].ToString() == "on"; 
                features.HAllowsCooking = Request.Form["features.HAllowsCooking"].ToString() == "on";
                features.HHasFurniture = Request.Form["features.HHasFurniture"].ToString() == "on";
                features.HInternet = Request.Form["features.HInternet"].ToString() == "on";
                features.HAirConditioning = Request.Form["features.HAirConditioning"].ToString() == "on";
                features.HSharedRental = Request.Form["features.HSharedRental"].ToString() == "on";
                features.HTv = Request.Form["features.HTv"].ToString() == "on";
                features.HRefrigerator = Request.Form["features.HRefrigerator"].ToString() == "on";
                features.HWashingMachine = Request.Form["features.HWashingMachine"].ToString() == "on";
                features.HBed = Request.Form["features.HBed"].ToString() == "on";
                features.HWaterHeater = Request.Form["features.HWaterHeater"].ToString() == "on";
                features.HGasStove = Request.Form["features.HGasStove"].ToString() == "on";
                features.HCableTv = Request.Form["features.HCableTv"].ToString() == "on";
                features.HWaterDispenser = Request.Form["features.HWaterDispenser"].ToString() == "on";
                features.HParking = Request.Form["features.HParking"].ToString() == "on";
                features.HSocialHousing = Request.Form["features.HSocialHousing"].ToString() == "on";
                features.HShortTermRent = Request.Form["features.HShortTermRent"].ToString() == "on";
                features.HPublicElectricity = Request.Form["features.HPublicElectricity"].ToString() == "on";
                features.HPublicWatercharges = Request.Form["features.HPublicWatercharges"].ToString() == "on";
                features.HLandlordShared = Request.Form["features.HLandlordShared"].ToString() == "on";
                features.HBalcony = Request.Form["features.HBalcony"].ToString() == "on";
                features.HPublicEquipment = Request.Form["features.HPublicEquipment"].ToString() == "on";

                // 驗證房東是否存在且已驗證
                var landlord = await _context.HLandlords
                    .FirstOrDefaultAsync(l => l.HLandlordId == property.HLandlordId);
                
                if (landlord == null)
                {
                    ModelState.AddModelError("HLandlordId", "選擇的房東不存在");
                    ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName");
                    return View(property);
                }

                // 開始交易
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 保存房源基本資訊
                        _context.Add(property);
                        await _context.SaveChangesAsync();

                        // 設置房源特色的關聯
                        features.HPropertyId = property.HPropertyId;
                        features.HLandlordId = property.HLandlordId;

                        // 保存房源特色
                        _context.Add(features);
                        await _context.SaveChangesAsync();

                        // 創建審核記錄
                        var propertyAudit = new HPropertyAudit
                        {
                            HPropertyId = property.HPropertyId,
                            HLandlordId = property.HLandlordId,
                            HAuditStatus = "已驗證",
                            HAuditDate = DateTime.Now,
                            HAuditNotes = "新建立的房源，等待管理員審核"
                        };
                        _context.HPropertyAudits.Add(propertyAudit);
                        await _context.SaveChangesAsync();

                        // 處理照片上傳
                        if (images != null && images.Count > 0)
                        {
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Property");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            foreach (var image in images)
                            {
                                if (image.Length > 0)
                                {
                                    // 生成唯一的檔案名
                                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                    // 保存檔案
                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await image.CopyToAsync(fileStream);
                                    }

                                    // 創建圖片記錄
                                    var propertyImage = new HPropertyImage
                                    {
                                        HPropertyId = property.HPropertyId,
                                        HLandlordId = property.HLandlordId,
                                        HImageUrl = "/images/Property/" + uniqueFileName,
                                        HUploadedDate = DateTime.Now,
                                        HLastUpDated = DateTime.Now
                                    };

                                    _context.HPropertyImages.Add(propertyImage);
                                }
                            }
                            await _context.SaveChangesAsync();
                        }

                        // 提交交易
                        await transaction.CommitAsync();

                        // 設置成功訊息
                        TempData["SuccessMessage"] = "房源新增成功！";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        // 回滾交易
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error occurred while creating property: {0}", ex.Message);
                        ModelState.AddModelError("", "創建房源時發生錯誤: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating property: {0}", ex.Message);
                ModelState.AddModelError("", "創建房源時發生錯誤: " + ex.Message);
            }

            // 如果新增失敗，重新載入房東列表並返回視圖
            ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName", property.HLandlordId);
            return View(property);
        }

        /// <summary>
        /// 顯示編輯房源頁面
        /// </summary>
        /// <param name="id">房源ID</param>
        public async Task<IActionResult> Edit(int? id)
        {
                if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var property = await _context.HProperties
                    .Include(p => p.HLandlord)
                    .Include(p => p.HPropertyFeatures)  // 加入房源特色
                    .Include(p => p.HPropertyImages)    // 加入房源照片
                    .Where(p => p.HStatus == "已驗證" && p.HLandlord.HStatus == "已驗證")
                    .FirstOrDefaultAsync(m => m.HPropertyId == id);
                
                if (property == null)
                {
                    return NotFound();
                }

                ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName", property.HLandlordId);
                return View(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing edit view");
                return View("Error");
            }
        }

        /// <summary>
        /// 處理編輯房源的請求
        /// </summary>
        /// <param name="id">房源ID</param>
        /// <param name="property">更新後的房源資料</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HProperty property, List<IFormFile> images, List<int> deletedImageIds)
        {
            if (id != property.HPropertyId)
            {
                return NotFound();
            }

            try
            {
                // 設置更新時間
                property.HLastUpdated = DateTime.Now;
                
                // 獲取原始房源資料以保留未修改的欄位
                var existingProperty = await _context.HProperties
                    .Include(p => p.HPropertyFeatures)
                    .Include(p => p.HPropertyImages)
                    .FirstOrDefaultAsync(p => p.HPropertyId == id);
                    
                if (existingProperty == null)
                {
                    return NotFound();
                }

                // 保留原有的狀態和發布日期
                property.HStatus = existingProperty.HStatus;
                property.HPublishedDate = existingProperty.HPublishedDate;

                // 開始交易
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 更新房源基本資訊
                        _context.Entry(existingProperty).CurrentValues.SetValues(property);
                        await _context.SaveChangesAsync();
                        
                        // 處理要刪除的照片
                        if (deletedImageIds != null && deletedImageIds.Any())
                        {
                            var imagesToDelete = await _context.HPropertyImages
                                .Where(img => deletedImageIds.Contains(img.HImageId))
                                .ToListAsync();

                            foreach (var image in imagesToDelete)
                            {
                                if (!string.IsNullOrEmpty(image.HImageUrl))
                                {
                                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.HImageUrl.TrimStart('/'));
                                    if (System.IO.File.Exists(imagePath))
                                    {
                                        System.IO.File.Delete(imagePath);
                                    }
                                }
                            }
                            _context.HPropertyImages.RemoveRange(imagesToDelete);
                            await _context.SaveChangesAsync();
                        }

                        // 更新房源特色
                        var features = existingProperty.HPropertyFeatures.FirstOrDefault();
                        
                        if (features != null)
                        {
                            // 處理 checkbox 值
                            try
                            {
                                features.HAllowsAnimals = Request.Form["features.HAllowsAnimals"].ToString() == "on";
                                features.HAllowsDogs = Request.Form["features.HAllowsDogs"].ToString() == "on";
                                features.HAllowsCats = Request.Form["features.HAllowsCats"].ToString() == "on";
                                features.HAllowsCooking = Request.Form["features.HAllowsCooking"].ToString() == "on";
                                features.HHasFurniture = Request.Form["features.HHasFurniture"].ToString() == "on";
                                features.HInternet = Request.Form["features.HInternet"].ToString() == "on";
                                features.HAirConditioning = Request.Form["features.HAirConditioning"].ToString() == "on";
                                features.HSharedRental = Request.Form["features.HSharedRental"].ToString() == "on";
                                features.HTv = Request.Form["features.HTv"].ToString() == "on";
                                features.HRefrigerator = Request.Form["features.HRefrigerator"].ToString() == "on";
                                features.HWashingMachine = Request.Form["features.HWashingMachine"].ToString() == "on";
                                features.HBed = Request.Form["features.HBed"].ToString() == "on";
                                features.HWaterHeater = Request.Form["features.HWaterHeater"].ToString() == "on";
                                features.HGasStove = Request.Form["features.HGasStove"].ToString() == "on";
                                features.HCableTv = Request.Form["features.HCableTv"].ToString() == "on";
                                features.HWaterDispenser = Request.Form["features.HWaterDispenser"].ToString() == "on";
                                features.HParking = Request.Form["features.HParking"].ToString() == "on";
                                features.HSocialHousing = Request.Form["features.HSocialHousing"].ToString() == "on";
                                features.HShortTermRent = Request.Form["features.HShortTermRent"].ToString() == "on";
                                features.HPublicElectricity = Request.Form["features.HPublicElectricity"].ToString() == "on";
                                features.HPublicWatercharges = Request.Form["features.HPublicWatercharges"].ToString() == "on";
                                features.HLandlordShared = Request.Form["features.HLandlordShared"].ToString() == "on";
                                features.HBalcony = Request.Form["features.HBalcony"].ToString() == "on";
                                features.HPublicEquipment = Request.Form["features.HPublicEquipment"].ToString() == "on";
                                
                                _context.Update(features);
                                await _context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error updating property features: {0}", ex.Message);
                            }
                        }

                        // 處理新上傳的照片
                        if (images != null && images.Count > 0 && images[0].Length > 0)
                        {
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Property");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            foreach (var image in images)
                            {
                                if (image.Length > 0)
                                {
                                    try
                                    {
                                        // 生成唯一的檔案名
                                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                        // 保存檔案
                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await image.CopyToAsync(fileStream);
                                        }

                                        // 創建圖片記錄
                                        var propertyImage = new HPropertyImage
                                        {
                                            HPropertyId = property.HPropertyId,
                                            HLandlordId = property.HLandlordId,
                                            HImageUrl = "/images/Property/" + uniqueFileName,
                                            HUploadedDate = DateTime.Now,
                                            HLastUpDated = DateTime.Now
                                        };

                                        _context.HPropertyImages.Add(propertyImage);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error saving image: {0}", ex.Message);
                                    }
                                }
                            }
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "房源更新成功！";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error occurred while updating property: {0}", ex.Message);
                        ModelState.AddModelError("", "更新房源時發生錯誤: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating property: {0}", ex.Message);
                ModelState.AddModelError("", "更新房源時發生錯誤: " + ex.Message);
            }

            try
            {
                // 如果更新失敗，重新載入資料
                var refreshedProperty = await _context.HProperties
                    .Include(p => p.HLandlord)
                    .Include(p => p.HPropertyFeatures)
                    .Include(p => p.HPropertyImages)
                    .FirstOrDefaultAsync(p => p.HPropertyId == id);

                ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName", property.HLandlordId);
                return View(refreshedProperty ?? property);
            }
            catch
            {
                // 最後的後備方案
                ViewData["HLandlordId"] = new SelectList(_context.HLandlords, "HLandlordId", "HLandlordName", property.HLandlordId);
                return View(property);
            }
        }

        /// <summary>
        /// 顯示刪除確認頁面
        /// </summary>
        /// <param name="id">房源ID</param>
        public async Task<IActionResult> Delete(int? id)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var property = await _context.HProperties
                    .Include(p => p.HLandlord)
                    .Include(p => p.HPropertyAudits)
                    .Include(p => p.HPropertyFeatures)
                    .Include(p => p.HPropertyImages)
                    .FirstOrDefaultAsync(m => m.HPropertyId == id);
                
                if (property == null)
                {
                    return NotFound();
                }

                return View(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing delete view");
                return View("Error");
            }
        }

        /// <summary>
        /// 處理刪除房源的請求
        /// </summary>
        /// <param name="id">房源ID</param>
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
                            .Include(p => p.HPropertyAudits)
                            .Include(p => p.HPropertyFeatures)
                            .Include(p => p.HPropertyImages)
                            .FirstOrDefaultAsync(p => p.HPropertyId == id);

                        if (property == null)
                        {
                            return NotFound();
                        }

                        // 刪除房源審核記錄
                        if (property.HPropertyAudits != null)
                        {
                            _context.HPropertyAudits.RemoveRange(property.HPropertyAudits);
                        }

                        // 刪除房源特色
                        if (property.HPropertyFeatures != null)
                        {
                            _context.HPropertyFeatures.RemoveRange(property.HPropertyFeatures);
                        }

                        // 刪除房源圖片
                        if (property.HPropertyImages != null)
                        {
                            // 刪除實體圖片文件
                            foreach (var image in property.HPropertyImages)
                            {
                                if (!string.IsNullOrEmpty(image.HImageUrl))
                                {
                                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.HImageUrl.TrimStart('/'));
                                    if (System.IO.File.Exists(imagePath))
                                    {
                                        System.IO.File.Delete(imagePath);
                                    }
                                }
                            }
                            _context.HPropertyImages.RemoveRange(property.HPropertyImages);
                        }

                        // 刪除房源本身
                        _context.HProperties.Remove(property);

                        // 保存所有更改
                        await _context.SaveChangesAsync();

                        // 提交交易
                        await transaction.CommitAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        // 回滾交易
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error occurred while deleting property and its related data");
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting property");
                return View("Error");
            }
        }

        /// <summary>
        /// 檢查房源是否存在
        /// </summary>
        /// <param name="id">房源ID</param>
        /// <returns>是否存在</returns>
        private bool PropertyExists(int id)
        {
            return _context.HProperties.Any(e => e.HPropertyId == id);
        }
    }
}

