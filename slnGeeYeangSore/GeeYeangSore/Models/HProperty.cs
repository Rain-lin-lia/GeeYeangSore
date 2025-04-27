using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HProperty
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

    public virtual ICollection<HAd> HAds { get; set; } = new List<HAd>();

    public virtual ICollection<HChat> HChats { get; set; } = new List<HChat>();

    public virtual ICollection<HFavorite> HFavorites { get; set; } = new List<HFavorite>();

    public virtual HLandlord HLandlord { get; set; } = null!;

    public virtual ICollection<HPropertyAudit> HPropertyAudits { get; set; } = new List<HPropertyAudit>();

    public virtual ICollection<HPropertyFeature> HPropertyFeatures { get; set; } = new List<HPropertyFeature>();

    public virtual ICollection<HPropertyImage> HPropertyImages { get; set; } = new List<HPropertyImage>();

    public virtual ICollection<HTransaction> HTransactions { get; set; } = new List<HTransaction>();
}
