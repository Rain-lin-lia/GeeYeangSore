using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HReport
{
    public int HReportId { get; set; }

    public int? HMessageId { get; set; }

    public int? HAuthorId { get; set; }

    public string? HAuthorType { get; set; }

    public string? HReason { get; set; }

    public string? HStatus { get; set; }

    public int? HAdminId { get; set; }

    public DateTime? HReviewedAt { get; set; }

    public DateTime? HCreatedAt { get; set; }

    public string? HReportType { get; set; }

    public int? HRelatedChatId { get; set; }

    public string? HAdminNote { get; set; }

    public int? HReportedUserId { get; set; }

    public virtual HAdmin? HAdmin { get; set; }

    public virtual HMessage? HMessage { get; set; }

    public virtual HTenant? HReportedUser { get; set; }
}
