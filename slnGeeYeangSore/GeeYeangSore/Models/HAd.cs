using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HAd
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

    public int? HPlanId { get; set; }

    public virtual HLandlord HLandlord { get; set; } = null!;

    public virtual HAdPlan? HPlan { get; set; }

    public virtual HProperty HProperty { get; set; } = null!;

    public virtual ICollection<HTransaction> HTransactions { get; set; } = new List<HTransaction>();
}
