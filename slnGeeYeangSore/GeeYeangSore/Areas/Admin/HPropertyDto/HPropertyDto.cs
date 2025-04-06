using GeeYeangSore.Models;

namespace GeeYeangSore.Areas.Admin.HPropertyDto
{
    public class HPropertyDto
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

        public bool HIsVip { get; set; }

        public bool HIsShared { get; set; }

        public virtual ICollection<HAd> HAds { get; set; } = new List<HAd>();

        public virtual ICollection<HFavorite> HFavorites { get; set; } = new List<HFavorite>();

        public virtual HLandlord HLandlord { get; set; } = null!;
    }
}
