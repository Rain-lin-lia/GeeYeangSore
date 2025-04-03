﻿using System;
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
}
