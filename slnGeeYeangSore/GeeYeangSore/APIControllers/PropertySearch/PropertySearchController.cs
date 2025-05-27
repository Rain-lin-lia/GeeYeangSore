using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.APIControllers;
using GeeYeangSore.Models;
using Microsoft.EntityFrameworkCore;
using GeeYeangSore.DTO.PropertySearch;

namespace GeeYeangSore.APIControllers.PropertySearch
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertySearchController : BaseController
    {
        public PropertySearchController(GeeYeangSoreContext db) : base(db) { }
        public static class PropertyExtensions
        {
            public static string? GetBadgeType(HProperty p, GeeYeangSoreContext db)
            {
                if (db.HAds.Any(ad => ad.HPropertyId == p.HPropertyId && ad.HCategory == "VIP3" && ad.HStatus == "進行中"))
                    return "精選";
                if (db.HAds.Any(ad => ad.HPropertyId == p.HPropertyId && ad.HCategory == "VIP2" && ad.HStatus == "進行中"))
                    return "推薦";
                if (p.HPublishedDate >= DateTime.Now.AddDays(-7))
                    return "New";
                return null;
            }
        }

        [HttpGet("propertyList")]
        public IActionResult GetPropertiesList()
        {
            var adMap = _db.HAds
                .Where(ad => ad.HStatus == "進行中" && (ad.HCategory == "VIP2" || ad.HCategory == "VIP3"))
                .Select(ad => new { ad.HPropertyId, ad.HCategory })
                .ToList()
                .GroupBy(ad => ad.HPropertyId)
                .ToDictionary(g => g.Key, g => g.First().HCategory);

            var now = DateTime.Now;
            var weekAgo = now.AddDays(-7);

            var properties = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Where(p => p.HAvailabilityStatus == "未出租" && p.HStatus == "已驗證" && p.HIsDelete == false &&
                _db.HAds.Any(ad =>
                        ad.HPropertyId == p.HPropertyId &&
                        ad.HStatus == "進行中"
                ))
                .OrderByDescending(p => p.HPublishedDate)
                .ToList();

            var result = properties.Select(p => new PropertyCardDTO
            {
                PropertyId = p.HPropertyId,
                Title = p.HPropertyTitle,
                RentPrice = p.HRentPrice,
                City = p.HCity,
                District = p.HDistrict,
                Address = p.HAddress,
                RoomCount = p.HRoomCount ?? 0,
                BathroomCount = p.HBathroomCount ?? 0,
                PropertyType = p.HPropertyType,
                Image = p.HPropertyImages
                            .Where(i => i.HIsDelete == false)
                            .OrderBy(i => i.HUploadedDate)
                            .Select(i => "https://localhost:7022" + i.HImageUrl)
                            .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg",

                BadgeType = adMap.ContainsKey(p.HPropertyId)
                    ? (adMap[p.HPropertyId] == "VIP3" ? "精選" : "推薦")
                    : (p.HPublishedDate >= weekAgo ? "New" : null)
            }).ToList();
            result = result
                     .OrderBy(p =>
                         p.BadgeType == "精選" ? 1 :
                         p.BadgeType == "推薦" ? 2 :
                         p.BadgeType == "New" ? 3 : 4
                     )
                     .ToList();

            return Ok(result);
        }

        [HttpGet("featuredProperties")]
        public IActionResult GetFeaturedProperties()
        {
            var data = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Where(p =>
                    p.HAvailabilityStatus == "未出租" &&
                    p.HStatus == "已驗證" &&
                    p.HIsDelete == false &&
                    _db.HAds.Any(ad =>
                        ad.HPropertyId == p.HPropertyId &&
                        ad.HCategory == "VIP3" &&
                        ad.HStatus == "進行中"
                    )
                )
                .OrderByDescending(p => Guid.NewGuid()) // 隨機
                .Take(6)
                .Select(p => new {
                    propertyId = p.HPropertyId,
                    title = p.HPropertyTitle,
                    rentPrice = p.HRentPrice,
                    city = p.HCity,
                    district = p.HDistrict,
                    address = p.HAddress,
                    roomCount = p.HRoomCount,
                    bathroomCount = p.HBathroomCount,
                    propertyType = p.HPropertyType,
                    image = p.HPropertyImages
                                .Where(i => i.HIsDelete == false)
                                .OrderBy(i => i.HUploadedDate)
                                .Select(i => "https://localhost:7022" + i.HImageUrl)
                                .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg",
                    BadgeType = "精選"
                })
                .ToList();

            return Ok(data);
        }

        [HttpGet("landlordProperties")]
        public IActionResult GetLandlordProperties()
        {
            var data = (
                from p in _db.HProperties
                join l in _db.HLandlords on p.HLandlordId equals l.HLandlordId
                join t in _db.HTenants on l.HTenantId equals t.HTenantId
                join ad in _db.HAds on p.HPropertyId equals ad.HPropertyId
                where
                    p.HAvailabilityStatus == "未出租" &&
                    p.HStatus == "已驗證" &&
                    l.HStatus == "已驗證" &&
                    (ad.HCategory == "VIP2" || ad.HCategory == "VIP3") &&
                    ad.HStatus == "進行中"
                select new
                {
                    landlord = new
                    {
                        id = l.HLandlordId,
                        name = t.HUserName,
                        phone = t.HPhoneNumber,
                        email = t.HEmail,
                        avatar = string.IsNullOrEmpty(t.HImages)
                            ? "https://localhost:7022/images/User/default.png"
                            : "https://localhost:7022/images/User/" + t.HImages
                    },
                    property = new
                    {
                        propertyId = p.HPropertyId,
                        title = p.HPropertyTitle,
                        rentPrice = p.HRentPrice,
                        city = p.HCity,
                        district = p.HDistrict,
                        address = p.HAddress,
                        propertyType = p.HPropertyType,
                        roomCount = p.HRoomCount,
                        bathroomCount = p.HBathroomCount,
                        image = p.HPropertyImages
                                    .Where(i => i.HIsDelete == false)
                                    .OrderBy(i => i.HUploadedDate)
                                    .Select(i => "https://localhost:7022" + i.HImageUrl)
                                    .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg",
                        badgeType = ad.HCategory == "VIP3" ? "精選" : "推薦"
                    }
                })
                .ToList()
                .OrderBy(x => Guid.NewGuid())
                .OrderBy(x =>
                    x.property.badgeType == "精選" ? 1 :
                    x.property.badgeType == "推薦" ? 2 : 3
                )
                .Take(6)
                .ToList();

            return Ok(data);
        }

        [HttpPost("filter")]
        public IActionResult FilterProperties([FromBody] PropertyFilterDTO filter)
        {
            var adMap = _db.HAds
                .Where(ad => ad.HStatus == "進行中" && (ad.HCategory == "VIP2" || ad.HCategory == "VIP3"))
                .Select(ad => new { ad.HPropertyId, ad.HCategory })
                .ToList()
                .GroupBy(ad => ad.HPropertyId)
                .ToDictionary(g => g.Key, g => g.First().HCategory);

            var now = DateTime.Now;
            var weekAgo = now.AddDays(-7);

            var query = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Include(p => p.HPropertyFeatures)
                .Where(p => p.HAvailabilityStatus == "未出租" && p.HStatus == "已驗證" && p.HIsDelete == false &&
                _db.HAds.Any(ad =>
                        ad.HPropertyId == p.HPropertyId &&
                        ad.HStatus == "進行中"
                ));

            // 關鍵字
            if (!string.IsNullOrWhiteSpace(filter.Keyword?.Trim()))
            {
                var keyword = filter.Keyword.Trim();
                query = query.Where(p =>
                    p.HPropertyTitle.Contains(keyword) ||
                    p.HCity.Contains(keyword) ||
                    p.HDistrict.Contains(keyword) ||
                    p.HAddress.Contains(keyword));
            }

            // 城市 / 區域
            var city = filter.City?.Trim();
            if (!string.IsNullOrWhiteSpace(city) && city != "不限")
            {
                query = query.Where(p => p.HCity.Trim() == city);
            }

            var district = filter.District?.Trim();
            if (!string.IsNullOrWhiteSpace(district) && district != "不限")
            {
                query = query.Where(p => p.HDistrict.Trim() == district);
            }

            // 類型與型態
            var type = filter.Type?.Trim();
            if (!string.IsNullOrWhiteSpace(type) && type != "不限")
            {
                query = query.Where(p => p.HPropertyType.Trim() == type);
            }

            var buildingType = filter.BuildingType?.Trim();
            if (!string.IsNullOrWhiteSpace(buildingType) && buildingType != "不限")
            {
                query = query.Where(p => p.HBuildingType.Trim() == buildingType);
            }

            // 租金
            if (filter.RentMin.HasValue)
                query = query.Where(p => p.HRentPrice >= filter.RentMin.Value);

            if (filter.RentMax.HasValue)
                query = query.Where(p => p.HRentPrice <= filter.RentMax.Value);

            // 坪數
            if (filter.AreaMin.HasValue)
                query = query.Where(p => p.HArea >= filter.AreaMin.Value);

            if (filter.AreaMax.HasValue)
                query = query.Where(p => p.HArea <= filter.AreaMax.Value);

            // 特色篩選
            if (filter.Features != null && filter.Features.Any())
            {
                foreach (var feature in filter.Features)
                {
                    if (feature == "可養狗")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAllowsDogs != null && f.HAllowsDogs == true));
                    else if (feature == "可養貓")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAllowsCats != null && f.HAllowsCats == true));
                    else if (feature == "可養其他寵物")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAllowsAnimals != null && f.HAllowsAnimals == true));
                    else if (feature == "可開伙")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAllowsCooking != null && f.HAllowsCooking == true));
                    else if (feature == "有家具")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HHasFurniture != null && f.HHasFurniture == true));
                    else if (feature == "有床")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HBed != null && f.HBed == true));
                    else if (feature == "有陽台")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HBalcony != null && f.HBalcony == true));
                    else if (feature == "公設")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HPublicEquipment != null && f.HPublicEquipment == true));
                    else if (feature == "車位")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HParking != null && f.HParking == true));
                    else if (feature == "有冷氣")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HAirConditioning != null && f.HAirConditioning == true));
                    else if (feature == "有電視")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HTv != null && f.HTv == true));
                    else if (feature == "有冰箱")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HRefrigerator != null && f.HRefrigerator == true));
                    else if (feature == "有洗衣機")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HWashingMachine != null && f.HWashingMachine == true));
                    else if (feature == "有飲水機")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HWaterDispenser != null && f.HWaterDispenser == true));
                    else if (feature == "有熱水器")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HWaterHeater != null && f.HWaterHeater == true));
                    else if (feature == "有天然瓦斯")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HGasStove != null && f.HGasStove == true));
                    else if (feature == "有網路")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HInternet != null && f.HInternet == true));
                    else if (feature == "有第四台")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HCableTv != null && f.HCableTv == true));
                    else if (feature == "公家電費")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HPublicElectricity != null && f.HPublicElectricity == true));
                    else if (feature == "公家水費")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HPublicWatercharges != null && f.HPublicWatercharges == true));
                    else if (feature == "短期租賃")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HShortTermRent != null && f.HShortTermRent == true));
                    else if (feature == "社會住宅")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HSocialHousing != null && f.HSocialHousing == true));
                    else if (feature == "房東同住")
                        query = query.Where(p => p.HPropertyFeatures.Any(f => f.HLandlordShared != null && f.HLandlordShared == true));
                }
            }

            var result = query
                .ToList()
                .Select(p => new
                {
                    propertyId = p.HPropertyId,
                    title = p.HPropertyTitle,
                    rentPrice = p.HRentPrice,
                    city = p.HCity,
                    district = p.HDistrict,
                    address = p.HAddress,
                    propertyType = p.HPropertyType,
                    roomCount = p.HRoomCount,
                    bathroomCount = p.HBathroomCount,
                    image = p.HPropertyImages
                        .Where(i => i.HIsDelete == false)
                        .OrderBy(i => i.HUploadedDate)
                        .Select(i => "https://localhost:7022" + i.HImageUrl)
                        .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg",
                    badgeType = adMap.ContainsKey(p.HPropertyId)
                                ? (adMap[p.HPropertyId] == "VIP3" ? "精選" : "推薦")
                                : (p.HPublishedDate >= weekAgo ? "New" : null)

                })
                .OrderBy(p =>
                    p.badgeType == "精選" ? 1 :
                    p.badgeType == "推薦" ? 2 :
                    p.badgeType == "New" ? 3 : 4
                )
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetPropertyById(int id)
        {
            var property = _db.HProperties
                .Include(p => p.HPropertyImages)
                .Include(p => p.HPropertyFeatures)
                .Include(p => p.HLandlord)
                    .ThenInclude(l => l.HTenant)
                .FirstOrDefault(p => p.HPropertyId == id && p.HIsDelete == false && p.HAvailabilityStatus == "未出租" && p.HStatus == "已驗證");

            if (property == null)
            {
                return NotFound(new { message = "找不到該房源" });
            }

            var ad = _db.HAds
                    .Where(ad => ad.HPropertyId == property.HPropertyId && ad.HStatus == "進行中")
                    .OrderByDescending(ad => ad.HCreatedDate)
                    .FirstOrDefault();

            var badgeType = ad != null
                            ? (ad.HCategory == "VIP3" ? "精選" : (ad.HCategory == "VIP2" ? "推薦" : null))
                            : (property.HPublishedDate >= DateTime.Now.AddDays(-7) ? "New" : null);

            var feature = property.HPropertyFeatures.FirstOrDefault(f => f.HIsDelete == false);
            if (feature == null)
            {
                return NotFound(new { message = "該房源無有效的特色資料" });
            }

            var featureMap = new Dictionary<string, bool?>
            {
                { "dog", feature.HAllowsDogs },
                { "cat", feature.HAllowsCats },
                { "other_pet", feature.HAllowsAnimals },
                { "cooking", feature.HAllowsCooking },
                { "furniture", feature.HHasFurniture },
                { "bed", feature.HBed },
                { "balcony", feature.HBalcony },
                { "public_facility", feature.HPublicEquipment },
                { "parking", feature.HParking },
                { "aircon", feature.HAirConditioning },
                { "tv", feature.HTv },
                { "fridge", feature.HRefrigerator },
                { "washer", feature.HWashingMachine },
                { "water_dispenser", feature.HWaterDispenser },
                { "heater", feature.HWaterHeater },
                { "gas", feature.HGasStove },
                { "internet", feature.HInternet },
                { "cable_tv", feature.HCableTv },
                { "public_elec", feature.HPublicElectricity },
                { "public_water", feature.HPublicWatercharges },
                { "short_term", feature.HShortTermRent },
                { "social_housing", feature.HSocialHousing },
                { "landlord_live", feature.HLandlordShared }
            };

            var features = featureMap
                .Where(kv => kv.Value.HasValue && kv.Value.Value)
                .Select(kv => kv.Key)
                .ToList();

            var result = new
            {
                property = new
                {
                    propertyId = property.HPropertyId,
                    title = property.HPropertyTitle,
                    description = property.HDescription,
                    address = property.HAddress,
                    city = property.HCity,
                    district = property.HDistrict,
                    zipcode = property.HZipcode,
                    rentPrice = property.HRentPrice,
                    propertyType = property.HPropertyType,
                    roomCount = property.HRoomCount,
                    bathroomCount = property.HBathroomCount,
                    area = property.HArea,
                    floor = property.HFloor,
                    totalFloors = property.HTotalFloors,
                    buildingType = property.HBuildingType,
                    publishedDate = property.HPublishedDate,
                    features = features,
                    badgeType = badgeType
                },
                images = property.HPropertyImages
                    .Where(i => i.HIsDelete == false)
                    .OrderBy(i => i.HUploadedDate)
                    .Select(i => "https://localhost:7022" + i.HImageUrl)
                    .ToList(),
                landlord = new
                {
                    id = property.HLandlord.HLandlordId,
                    name = property.HLandlord.HTenant.HUserName,
                    phone = property.HLandlord.HTenant.HPhoneNumber,
                    email = property.HLandlord.HTenant.HEmail,
                    avatar = string.IsNullOrEmpty(property.HLandlord.HTenant.HImages)
                        ? "https://localhost:7022/images/User/default.png"
                        : "https://localhost:7022/images/User/" + property.HLandlord.HTenant.HImages
                }
            };

            return Ok(result);
        }

        [HttpGet("recommendedProperties")]
        public IActionResult GetRecommendedProperties()
        {
            var adMap = _db.HAds
                        .Where(ad => ad.HStatus == "進行中" && (ad.HCategory == "VIP1" || ad.HCategory == "VIP2" || ad.HCategory == "VIP3"))
                        .Select(ad => new { ad.HPropertyId, ad.HCategory })
                        .ToList()
                        .GroupBy(ad => ad.HPropertyId)
                        .ToDictionary(g => g.Key, g => g.First().HCategory);

            var now = DateTime.Now;
            var weekAgo = now.AddDays(-7);

            var properties = _db.HProperties
                            .Include(p => p.HPropertyImages)
                            .Where(p =>
                                p.HAvailabilityStatus == "未出租" &&
                                p.HStatus == "已驗證" &&
                                p.HIsDelete == false &&
                                _db.HAds.Any(ad =>
                                    ad.HPropertyId == p.HPropertyId &&
                                    ad.HStatus == "進行中"
                                )
                            )
                            .ToList()
                            .Select(p => new
                            {
                                propertyId = p.HPropertyId,
                                title = p.HPropertyTitle,
                                rentPrice = p.HRentPrice,
                                city = p.HCity,
                                district = p.HDistrict,
                                address = p.HAddress,
                                roomCount = p.HRoomCount,
                                bathroomCount = p.HBathroomCount,
                                propertyType = p.HPropertyType,
                                image = p.HPropertyImages
                                    .Where(i => i.HIsDelete == false)
                                    .OrderBy(i => i.HUploadedDate)
                                    .Select(i => "https://localhost:7022" + i.HImageUrl)
                                    .FirstOrDefault() ?? "https://localhost:7022/images/Property/1.jpg",
                                badgeType = adMap.ContainsKey(p.HPropertyId)
                                    ? (adMap[p.HPropertyId] == "VIP3" ? "精選" : "推薦")
                                    : (p.HPublishedDate >= weekAgo ? "New" : null)
                            })
                            .OrderBy(p => Guid.NewGuid())
                            .OrderBy(p =>
                                p.badgeType == "精選" ? 1 :
                                p.badgeType == "推薦" ? 2 :
                                p.badgeType == "New" ? 3 : 4
                            )
                            .Take(6)
                            .ToList();
            return Ok(properties);
        }
    }
}
