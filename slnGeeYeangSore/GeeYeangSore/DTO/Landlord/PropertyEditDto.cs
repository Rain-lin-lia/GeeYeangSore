using GeeYeangSore.Models;

namespace GeeYeangSore.DTO.Landlord
{
    public class PropertyEditDto
    {
        public string HPropertyTitle { get; set; }
        public string HDescription { get; set; }
        public string HAddress { get; set; }
        public string HCity { get; set; }
        public string HDistrict { get; set; }
        public string HZipcode { get; set; }
        public decimal HRentPrice { get; set; }
        public string HPropertyType { get; set; }
        public int HRoomCount { get; set; }
        public int HBathroomCount { get; set; }
        public decimal HArea { get; set; }
        public int HFloor { get; set; }
        public int HTotalFloors { get; set; }
        public string HAvailabilityStatus { get; set; }
        public string HBuildingType { get; set; }
        public bool HIsVip { get; set; }
        public bool HIsShared { get; set; }
        public string HStatus { get; set; }
        public decimal? HLatitude { get; set; }
        public decimal? HLongitude { get; set; }
        public PropertyFeatureDto Feature { get; set; }
    }

    public class PropertyFeatureDto
    {
        public bool? HAllowsDogs { get; set; }
        public bool? HAllowsCats { get; set; }
        public bool? HAllowsAnimals { get; set; }
        public bool? HAllowsCooking { get; set; }
        public bool? HHasFurniture { get; set; }
        public bool? HInternet { get; set; }
        public bool? HAirConditioning { get; set; }
        public bool? HSharedRental { get; set; }
        public bool? HTv { get; set; }
        public bool? HRefrigerator { get; set; }
        public bool? HWashingMachine { get; set; }
        public bool? HBed { get; set; }
        public bool? HWaterHeater { get; set; }
        public bool? HGasStove { get; set; }
        public bool? HCableTv { get; set; }
        public bool? HWaterDispenser { get; set; }
        public bool? HParking { get; set; }
        public bool? HSocialHousing { get; set; }
        public bool? HShortTermRent { get; set; }
        public bool? HPublicElectricity { get; set; }
        public bool? HPublicWatercharges { get; set; }
        public bool? HLandlordShared { get; set; }
        public bool? HBalcony { get; set; }
        public bool? HPublicEquipment { get; set; }
    }

    public class PropertyDto
    {
        public int HPropertyId { get; set; }
        public int HLandlordId { get; set; }
        public string? HPropertyTitle { get; set; }
        public string? HDescription { get; set; }
        public string? HAddress { get; set; }
        public string? HCity { get; set; }
        public string? HDistrict { get; set; }
        public string? HZipcode { get; set; }
        public int? HRentPrice { get; set; }
        public string? HPropertyType { get; set; }
        public int? HRoomCount { get; set; }
        public int? HBathroomCount { get; set; }
        public int? HArea { get; set; }
        public int? HFloor { get; set; }
        public int? HTotalFloors { get; set; }
        public string? HAvailabilityStatus { get; set; }
        public string? HBuildingType { get; set; }
        public string? HScore { get; set; }
        public DateTime? HPublishedDate { get; set; }
        public DateTime? HLastUpdated { get; set; }
        public bool? HIsVip { get; set; }
        public bool? HIsShared { get; set; }
        public string? HStatus { get; set; }
        public bool? HIsDelete { get; set; }
        public string? HLatitude { get; set; }
        public string? HLongitude { get; set; }
    }

    public class HPropertyImageDto
    {
        public int HImageId { get; set; }
        public int? HPropertyId { get; set; }
        public int? HLandlordId { get; set; }
        public string? HImageUrl { get; set; }
        public string? HCaption { get; set; }
        public DateTime? HUploadedDate { get; set; }
        public DateTime? HLastUpDated { get; set; }
        public bool? HIsDelete { get; set; }
        public virtual HLandlord? HLandlord { get; set; }
        public virtual HProperty? HProperty { get; set; }
    }

    public class HAdDto
    {
        public int HAdId { get; set; }
        public int HLandlordId { get; set; }
        public int HPropertyId { get; set; }
        public string? HAdName { get; set; }
        public string? HDescription { get; set; }
        public string? HCategory { get; set; }
        public int? HPriority { get; set; }
        public string? HAdTag { get; set; }
        public string? HTargetRegion { get; set; }
        public string? HStatus { get; set; }
        public decimal? HAdPrice { get; set; }
        public string? HLinkUrl { get; set; }
        public string? HImageUrl { get; set; }
        public DateTime? HStartDate { get; set; }
        public DateTime? HEndDate { get; set; }
        public bool? HIsDelete { get; set; }
        public DateTime? HCreatedDate { get; set; }
        public DateTime? HLastUpdated { get; set; }
    }
}