﻿using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HLandlord
{
    public int HLandlordId { get; set; }

    public int HTenantId { get; set; }

    public string HLandlordName { get; set; } = null!;

    public string HBankName { get; set; } = null!;

    public string HBankAccount { get; set; } = null!;

    public string HIdCardFrontUrl { get; set; } = null!;

    public string HIdCardBackUrl { get; set; } = null!;

    public DateTime HCreatedAt { get; set; }

    public DateTime HUpdateAt { get; set; }

    public string HStatus { get; set; } = null!;

    public bool HIsDeleted { get; set; }

    public virtual ICollection<HAd> HAds { get; set; } = new List<HAd>();

    public virtual ICollection<HLblacklist> HLblacklists { get; set; } = new List<HLblacklist>();

    public virtual ICollection<HProperty> HProperties { get; set; } = new List<HProperty>();

    public virtual ICollection<HPropertyAudit> HPropertyAudits { get; set; } = new List<HPropertyAudit>();

    public virtual ICollection<HPropertyFeature> HPropertyFeatures { get; set; } = new List<HPropertyFeature>();

    public virtual ICollection<HPropertyImage> HPropertyImages { get; set; } = new List<HPropertyImage>();

    public virtual HTenant HTenant { get; set; } = null!;
}
