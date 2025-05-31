using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HSso
{
    public int HSsoId { get; set; }

    public int HTenantId { get; set; }

    public string HSub { get; set; } = null!;

    public string HAud { get; set; } = null!;

    public string HUserEmail { get; set; } = null!;

    public bool HEmailverified { get; set; }

    public DateTime HExp { get; set; }

    public DateTime HIat { get; set; }

    public virtual HTenant HTenant { get; set; } = null!;
}
