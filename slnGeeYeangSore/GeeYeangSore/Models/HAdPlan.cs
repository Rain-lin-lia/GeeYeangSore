using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HAdPlan
{
    public int HPlanId { get; set; }

    public string HCategory { get; set; } = null!;

    public int HAdPrice { get; set; }

    public int HDays { get; set; }

    public string? HName { get; set; }

    public virtual ICollection<HAd> HAds { get; set; } = new List<HAd>();
}
