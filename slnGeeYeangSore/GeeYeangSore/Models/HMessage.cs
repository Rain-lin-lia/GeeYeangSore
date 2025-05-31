using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HMessage
{
    public int HMessageId { get; set; }

    public int? HChatId { get; set; }

    public int? HSenderId { get; set; }

    public string? HSenderRole { get; set; }

    public int? HReceiverId { get; set; }

    public string? HReceiverRole { get; set; }

    public string? HMessageType { get; set; }

    public string? HContent { get; set; }

    public string? HAttachmentUrl { get; set; }

    public int? HIsRead { get; set; }

    public DateTime? HTimestamp { get; set; }

    public string? HSource { get; set; }

    public int? HIsDeleted { get; set; }

    public int? HReportCount { get; set; }

    public int? HDeletedBySender { get; set; }

    public int? HDeletedByReceiver { get; set; }

    public DateTime? HEditedAt { get; set; }

    public int? HIsEdited { get; set; }

    public int? HReplyToMessageId { get; set; }

    public int? HPropertyId { get; set; }

    public virtual HChat? HChat { get; set; }

    public virtual HProperty? HProperty { get; set; }

    public virtual ICollection<HReaction> HReactions { get; set; } = new List<HReaction>();

    public virtual ICollection<HReport> HReports { get; set; } = new List<HReport>();
}
