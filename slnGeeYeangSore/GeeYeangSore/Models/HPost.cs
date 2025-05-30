﻿using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HPost
{
    public int HPostId { get; set; }

    public int? HAuthorId { get; set; }

    public string? HAuthorType { get; set; }

    public string? HTitle { get; set; }

    public string? HContent { get; set; }

    public string? HStatus { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public DateTime? HUpdatedAt { get; set; }

    public DateTime? HLastreplyTime { get; set; }

    public int? HIsPinned { get; set; }

    public int? HIsLocked { get; set; }

    public DateTime? HDeletedAt { get; set; }

    public virtual ICollection<HPostMonitoring> HPostMonitorings { get; set; } = new List<HPostMonitoring>();
}
