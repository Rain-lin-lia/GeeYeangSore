using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HReaction
{
    public int HReactionId { get; set; }

    public int? HTenantId { get; set; }

    public string? HAuthorType { get; set; }

    public int? HTargetId { get; set; }

    public string? HTargetType { get; set; }

    public string? HContentType { get; set; }

    public string? HReactionType { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public virtual HMessage? HTarget { get; set; }

    public virtual HTenant? HTenant { get; set; }
}
