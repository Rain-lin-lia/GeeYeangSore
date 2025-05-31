using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HTenant
{
    public int HTenantId { get; set; }

    public string HUserName { get; set; } = null!;

    public DateTime? HBirthday { get; set; }

    public bool? HGender { get; set; }

    public string? HAddress { get; set; }

    public string? HPhoneNumber { get; set; }

    public string HEmail { get; set; } = null!;

    public string? HPassword { get; set; }

    public string? HSalt { get; set; }

    public string? HImages { get; set; }

    public string HStatus { get; set; } = null!;

    public bool HIsTenant { get; set; }

    public bool HIsLandlord { get; set; }

    public DateTime HCreatedAt { get; set; }

    public DateTime HUpdateAt { get; set; }

    public bool HIsDeleted { get; set; }

    public string? HLastLoginIp { get; set; }

    public DateTime? HLastLoginAt { get; set; }

    public int HLoginFailCount { get; set; }

    public DateTime? HLockoutEnd { get; set; }

    public virtual ICollection<HFavorite> HFavorites { get; set; } = new List<HFavorite>();

    public virtual ICollection<HLandlord> HLandlords { get; set; } = new List<HLandlord>();

    public virtual ICollection<HMatch> HMatchHMatchEdusers { get; set; } = new List<HMatch>();

    public virtual ICollection<HMatch> HMatchHTenants { get; set; } = new List<HMatch>();

    public virtual ICollection<HMblacklist> HMblacklists { get; set; } = new List<HMblacklist>();

    public virtual ICollection<HNotify> HNotifies { get; set; } = new List<HNotify>();

    public virtual ICollection<HPasswordReset> HPasswordResets { get; set; } = new List<HPasswordReset>();

    public virtual ICollection<HReaction> HReactions { get; set; } = new List<HReaction>();

    public virtual ICollection<HReport> HReports { get; set; } = new List<HReport>();

    public virtual ICollection<HSso> HSsos { get; set; } = new List<HSso>();
}
