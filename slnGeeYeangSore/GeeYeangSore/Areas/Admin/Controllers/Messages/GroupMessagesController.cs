using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GeeYeangSore.Models;
using GeeYeangSore.Controllers;
using System.Collections.Generic;

namespace GeeYeangSore.Areas.Admin.Controllers
{
    /// <summary>
    /// 群組訊息控制器
    /// 負責處理群組聊天的相關功能，包括：
    /// 1. 群組列表顯示
    /// 2. 群組聊天內容查看
    /// 3. 群組訊息刪除
    /// 4. 訊息檢舉功能
    /// 5. 檢舉列表管理
    /// </summary>
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")]
    public class GroupMessagesController : SuperController
    {
        private readonly GeeYeangSoreContext _context;
        private const int PageSize = 10;

        public GroupMessagesController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 群組列表頁面
        /// 顯示所有群組聊天室，支援分頁和搜尋功能
        /// </summary>
        /// <param name="searchString">搜尋關鍵字，用於過濾群組訊息內容</param>
        /// <param name="page">當前頁碼，預設為第1頁</param>
        /// <returns>群組列表視圖，包含分頁資訊和搜尋結果</returns>
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 只顯示群組對話
            var query = _context.HMessages
                .Where(m => m.HChatId != null);

            // 如果有搜尋關鍵字，過濾訊息內容
            if (!string.IsNullOrEmpty(searchString?.Trim()))
            {
                var trimmedSearch = searchString.Trim();
                query = query.Where(m => m.HContent.Contains(trimmedSearch));
            }

            // 計算總頁數
            var totalItems = await query.Select(m => m.HChatId).Distinct().CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // 獲取當前頁的群組列表
            var chatGroups = await query
                .GroupBy(m => m.HChatId)
                .Select(g => new
                {
                    ChatId = g.Key,
                    LastMessageTime = g.Max(m => m.HTimestamp)
                })
                .OrderByDescending(g => g.LastMessageTime)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(chatGroups);
        }

        /// <summary>
        /// 群組聊天內容頁面
        /// 顯示特定群組的所有聊天訊息，並獲取發送者名稱
        /// </summary>
        /// <param name="chatId">要查看的群組ID</param>
        /// <returns>群組聊天內容視圖，包含訊息列表和發送者資訊</returns>
        [HttpGet]
        [Route("Admin/GroupMessages/GroupChat/{chatId}")]
        public async Task<IActionResult> GroupChat(int chatId)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 獲取該群組的所有訊息，包含檢舉資訊
            var messages = await _context.HMessages
                .Where(m => m.HChatId == chatId)
                .Include(m => m.HReports)  // 包含檢舉資訊
                .OrderBy(m => m.HTimestamp)
                .ToListAsync();

            // 獲取所有發送者的名稱
            var senderNames = new Dictionary<int, string>();
            foreach (var message in messages)
            {
                if (message.HSenderId.HasValue)
                {
                    // 先檢查是否為房客
                    var tenant = await _context.HTenants
                        .FirstOrDefaultAsync(t => t.HTenantId == message.HSenderId);

                    // 再檢查是否為房東
                    var landlord = await _context.HLandlords
                        .FirstOrDefaultAsync(l => l.HLandlordId == message.HSenderId);

                    // 根據角色和找到的資料決定顯示的名稱
                    if (message.HSenderRole == "Tenant" && tenant != null)
                    {
                        senderNames[message.HMessageId] = tenant.HUserName;
                    }
                    else if (message.HSenderRole == "Landlord" && landlord != null)
                    {
                        senderNames[message.HMessageId] = landlord.HLandlordName;
                    }
                    else if (tenant != null)
                    {
                        senderNames[message.HMessageId] = tenant.HUserName;
                    }
                    else if (landlord != null)
                    {
                        senderNames[message.HMessageId] = landlord.HLandlordName;
                    }
                    else
                    {
                        senderNames[message.HMessageId] = "系統";
                    }
                }
                else
                {
                    senderNames[message.HMessageId] = "系統";
                }
            }

            // 獲取群組名稱（如果有）
            var groupName = await _context.HChats
                .Where(c => c.HChatId == chatId)
                .Select(c => c.HChatName)
                .FirstOrDefaultAsync() ?? $"群組 {chatId}";

            ViewBag.ChatId = chatId;
            ViewBag.MessageCount = messages.Count;
            ViewBag.SenderNames = senderNames;
            ViewBag.GroupName = groupName;

            return View(messages);
        }

        /// <summary>
        /// 刪除群組訊息
        /// 刪除指定群組的所有聊天記錄
        /// </summary>
        /// <param name="chatId">要刪除的群組ID</param>
        /// <returns>JSON格式的操作結果，包含成功/失敗訊息</returns>
        [HttpPost]
        [Route("Admin/GroupMessages/Delete/{chatId}")]
        public async Task<IActionResult> Delete(int chatId)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                // 先刪除該群組的所有訊息
                var messages = await _context.HMessages
                    .Where(m => m.HChatId == chatId)
                    .ToListAsync();

                _context.HMessages.RemoveRange(messages);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "群組已成功刪除" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除失敗：" + ex.Message });
            }
        }

        /// <summary>
        /// 提交訊息檢舉
        /// 處理用戶對特定訊息的檢舉請求
        /// </summary>
        /// <param name="messageId">被檢舉的訊息ID</param>
        /// <param name="reason">檢舉原因</param>
        /// <returns>重定向到群組聊天頁面，並顯示操作結果訊息</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int messageId, string reason)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var message = await _context.HMessages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            var report = new HReport
            {
                HMessageId = messageId,
                HAuthorId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"),
                HAuthorType = "Admin",
                HReason = reason,
                HStatus = "待處理",
                HCreatedAt = DateTime.Now,
                HReportType = "Group"  // 設置為群組訊息檢舉
            };

            _context.HReports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "檢舉已成功提交！";
            return RedirectToAction("GroupChat", new { chatId = message.HChatId });
        }

        /// <summary>
        /// 檢舉列表頁面
        /// 顯示所有群組訊息的檢舉記錄，支援按狀態篩選
        /// </summary>
        /// <param name="status">檢舉狀態篩選條件，可選值：Pending（待處理）、Approved（已核准）、Rejected（已拒絕）</param>
        /// <returns>檢舉列表視圖，包含檢舉記錄和相關操作按鈕</returns>
        public async Task<IActionResult> ReportList(string status = null)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 只查詢群組訊息的檢舉
            var query = _context.HReports
                .Where(r => r.HReportType == "Group")
                .Include(r => r.HMessage)  // 包含訊息內容
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.HStatus == status);
            }

            var reports = await query
                .OrderByDescending(r => r.HCreatedAt)
                .ToListAsync();

            // 獲取每個檢舉對應的聊天 ID
            var chatIds = new Dictionary<int, int?>();
            foreach (var report in reports)
            {
                if (report.HMessageId.HasValue)
                {
                    var message = await _context.HMessages.FindAsync(report.HMessageId);
                    chatIds[report.HReportId] = message?.HChatId;
                }
            }

            ViewBag.Status = status;
            ViewBag.ChatIds = chatIds;
            return View(reports);
        }

        /// <summary>
        /// 更新檢舉狀態
        /// 處理管理員對檢舉記錄的審核操作
        /// </summary>
        /// <param name="reportId">要更新的檢舉ID</param>
        /// <param name="status">新的檢舉狀態，可選值：Approved（核准）、Rejected（拒絕）</param>
        /// <returns>JSON格式的操作結果，包含成功/失敗訊息</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateReportStatus(int reportId, string status)
        {
            if (!HasAnyRole("超級管理員", "內容管理員", "系統管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            try
            {
                // 先獲取當前管理員帳號
                if (string.IsNullOrEmpty(LoginedUser))
                {
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
                }

                // 使用管理員帳號查找管理員記錄
                var admin = await _context.HAdmins.FirstOrDefaultAsync(a => a.HAccount == LoginedUser);
                if (admin == null)
                {
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
                }

                // 獲取檢舉記錄
                var report = await _context.HReports.FindAsync(reportId);
                if (report == null)
                {
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
                }

                // 檢查狀態是否有效
                if (status != "已核准" && status != "已拒絕")
                {
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });
                }

                // 更新檢舉狀態
                report.HStatus = status;
                report.HAdminId = admin.HAdminId;
                report.HReviewedAt = DateTime.Now;

                try
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "狀態已更新" });
                }
                catch (DbUpdateException dbEx)
                {
                    if (dbEx.InnerException?.Message.Contains("FK_h_Reports_h_Admin") == true)
                    {
                        return Json(new { success = false, message = $"管理員ID {admin.HAdminId} 無法用於更新檢舉記錄" });
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新失敗：{ex.Message}" });
            }
        }
    }
}
