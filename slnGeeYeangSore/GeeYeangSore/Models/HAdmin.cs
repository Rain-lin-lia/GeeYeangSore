﻿using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HAdmin
{
    public int HAdminId { get; set; }

    public string? HRoleLevel { get; set; }

    public string? HAccount { get; set; }

    public string? HPassword { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public DateTime? HUpdateAt { get; set; }

    public virtual ICollection<HAdminLog> HAdminLogs { get; set; } = new List<HAdminLog>();

    public virtual ICollection<HContact> HContacts { get; set; } = new List<HContact>();

    public virtual ICollection<HPostMonitoring> HPostMonitorings { get; set; } = new List<HPostMonitoring>();

    public virtual ICollection<HReportForum> HReportForums { get; set; } = new List<HReportForum>();

    public virtual ICollection<HReport> HReports { get; set; } = new List<HReport>();
}
