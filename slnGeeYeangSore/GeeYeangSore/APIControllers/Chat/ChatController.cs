using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using GeeYeangSore.DTO.Chat;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GeeYeangSore.APIControllers.Chat
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : BaseController
    {
        public ChatController(GeeYeangSoreContext db) : base(db) { }
        [HttpGet("chatlist")]
        public async Task<IActionResult> GetChatList()
        {
            try
            {
                // 驗證登入與權限
                var access = CheckAccess();
                if (access != null) return access;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });

                var userId = tenant.HTenantId;

                // Step 1：先把所有與我有關的訊息撈出來（由於 GroupBy 無法轉換，先撈成記憶體）
                var allMessages = await _db.HMessages
                    .Where(m => m.HSenderId == userId || m.HReceiverId == userId)
                    .OrderByDescending(m => m.HTimestamp)
                    .ToListAsync();

                // Step 2：記憶體中分組，並取出每組的最新一筆訊息
                var latestMessages = allMessages
                    .GroupBy(m => m.HSenderId == userId ? m.HReceiverId : m.HSenderId)
                    .Select(g => g.OrderByDescending(m => m.HTimestamp).First())
                    .OrderByDescending(m => m.HTimestamp)
                    .ToList();

                // Step 3：抓出聯絡人 ID
                var contactIds = latestMessages
                    .Select(m => m.HSenderId == userId ? m.HReceiverId : m.HSenderId)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .Distinct()
                    .ToList();

                // Step 4：查聯絡人名稱與頭像
                var contactInfos = await _db.HTenants
                    .Where(t => contactIds.Contains(t.HTenantId))
                    .ToDictionaryAsync(
                        t => t.HTenantId,
                        t => new { t.HUserName, t.HImages }
                    );

                // Step 5：組合回傳資料
                var result = latestMessages.Select(m =>
                {
                    var targetId = m.HSenderId == userId ? m.HReceiverId : m.HSenderId;
                    var info = contactInfos.ContainsKey(targetId ?? 0) ? contactInfos[targetId ?? 0] : null;
                    return new
                    {
                        m.HMessageId,
                        HSenderId = targetId,
                        SenderName = info?.HUserName ?? $"聯絡人{targetId}",
                        Avatar = info?.HImages,
                        m.HContent,
                        m.HTimestamp
                    };
                }).ToList();

                return Ok(new { success = true, data = result, selfId = userId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }

        [HttpGet("history/{otherId}")]
        public async Task<IActionResult> GetChatHistory(int otherId, int? skip = 0, int? take = 20)
        {
            try
            {
                var access = CheckAccess();
                if (access != null) return access;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });

                var userId = tenant.HTenantId;
                var skipCount = Math.Max(skip ?? 0, 0);
                var takeCount = Math.Clamp(take ?? 20, 10, 100); // 限制最大查詢筆數

                // 查詢雙向訊息（先抓最新 N 筆，前端再排序成舊到新）
                var messages = await _db.HMessages
                    .Where(m =>
                        ((m.HSenderId == userId && m.HReceiverId == otherId) ||
                        (m.HSenderId == otherId && m.HReceiverId == userId)) &&
                        (m.HIsDeleted == null || m.HIsDeleted == 0)
                    )
                    .OrderByDescending(m => m.HTimestamp) // 最新在前
                    .Skip(skipCount)
                    .Take(takeCount)
                    .Select(m => new
                    {
                        m.HMessageId,
                        m.HSenderId,
                        m.HReceiverId,
                        m.HMessageType,
                        m.HContent,
                        m.HAttachmentUrl,
                        m.HTimestamp,
                        m.HIsRead,
                        m.HReplyToMessageId,
                        m.HIsEdited
                    })
                    .ToListAsync();

                // 前端顯示順序：從舊到新（所以這裡要 reverse）
                var orderedMessages = messages.OrderBy(m => m.HTimestamp).ToList();

                return Ok(new
                {
                    success = true,
                    data = orderedMessages,
                    hasMore = messages.Count == takeCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var access = CheckAccess();
                if (access != null) return access;

                var user = GetCurrentTenant();
                if (user == null) return Unauthorized(new { success = false, message = "未登入" });

                var userId = user.HTenantId;

                var unreadCounts = await _db.HMessages
                    .Where(m => m.HReceiverId == userId && m.HIsRead == 0)
                    .GroupBy(m => m.HSenderId)
                    .Select(g => new
                    {
                        SenderId = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = unreadCounts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] int senderId)
        {
            try
            {
                var access = CheckAccess();
                if (access != null) return access;

                var user = GetCurrentTenant();
                if (user == null) return Unauthorized(new { success = false, message = "未登入" });

                var userId = user.HTenantId;

                var unreadMessages = await _db.HMessages
                    .Where(m => m.HSenderId == senderId && m.HReceiverId == userId && m.HIsRead == 0)
                    .ToListAsync();

                foreach (var msg in unreadMessages)
                    msg.HIsRead = 1;

                await _db.SaveChangesAsync();

                return Ok(new { success = true, updated = unreadMessages.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
            }
        }

        [HttpPost("delete-message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                var access = CheckAccess();
                if (access != null) return access;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });

                var message = await _db.HMessages.FindAsync(messageId);
                if (message == null)
                    return NotFound(new { success = false, message = "訊息不存在" });

                message.HIsDeleted = 1;
                await _db.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "刪除失敗", error = ex.Message });
            }
        }

        [HttpPost("report-message")]
        public async Task<IActionResult> ReportMessage([FromBody] ReportMessageDto dto)
        {
            try
            {
                var access = CheckAccess();
                if (access != null) return access;

                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });

                var message = await _db.HMessages.FindAsync(dto.MessageId);
                if (message == null)
                    return NotFound(new { success = false, message = "訊息不存在" });

                // h_ReportCount +1
                message.HReportCount = (message.HReportCount ?? 0) + 1;

                // 新增 HReport
                var report = new HReport
                {
                    HMessageId = dto.MessageId,
                    HAuthorId = tenant.HTenantId,
                    HAuthorType = tenant.HIsLandlord ? "房東" : "房客",
                    HReason = dto.Reason ?? "不當內容",
                    HStatus = "待處理",
                    HCreatedAt = DateTime.Now,
                    HReportType = "私人"
                };
                _db.HReports.Add(report);

                await _db.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "檢舉失敗", error = ex.Message });
            }
        }

        // [HttpGet("avatar/{tenantId}")]
        // public async Task<IActionResult> GetTenantAvatar(int tenantId)
        // {
        //     try
        //     {
        //         var access = CheckAccess();
        //         if (access != null) return access;

        //         var tenant = await _db.HTenants
        //             .Where(t => t.HTenantId == tenantId && !t.HIsDeleted)
        //             .Select(t => new { tenantId = t.HTenantId, avatar = t.HImages })
        //             .FirstOrDefaultAsync();

        //         if (tenant == null)
        //             return NotFound(new { success = false, message = "找不到此用戶" });

        //         return Ok(new { success = true, data = tenant });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { success = false, message = "伺服器發生錯誤", error = ex.Message });
        //     }
        // }
    }
}
