using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HEmailToken
{
    public int HEmailTokenId { get; set; }

    public string HUserEmail { get; set; } = null!;

    public string HEmailToken1 { get; set; } = null!;

    public string? HEmailSalt { get; set; }

    public DateTime HResetExpiresAt { get; set; }

    public bool HIsUsed { get; set; }

    public DateTime HCreatedAt { get; set; }

    public DateTime? HUsedAt { get; set; }

    public string? HRequestIp { get; set; }

    public string HTokenType { get; set; } = null!;
}
