using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;
using GeeYeangSore.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace GeeYeangSore.Areas.Admin.Controllers.Messages
{
    /// <summary>
    /// 私人訊息管理控制器
    /// </summary>
    [Area("Admin")]
    public class PrivateMessagesController : SuperController
    {
        // 注入資料庫上下文
        private readonly GeeYeangSoreContext _context;
        // 設定每頁顯示10筆資料
        private const int PageSize = 10;

        // 建構函數，通過依賴注入獲取資料庫上下文
        public PrivateMessagesController(GeeYeangSoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 顯示私人訊息列表
        /// </summary>
        /// <param name="searchString">搜尋關鍵字</param>
        /// <param name="page">當前頁碼，預設為第1頁</param>
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 建立基礎查詢，只顯示有效的私人對話
            var query = _context.HMessages
                .Where(m =>
                    m.HChatId == null && // 只顯示私人對話
                    m.HReceiverId != null && m.HReceiverId != 0); // 接收者必須存在且不為0

            // 如果有搜尋關鍵字，則進行篩選
            if (!string.IsNullOrEmpty(searchString?.Trim()))
            {
                var trimmedSearch = searchString.Trim();
                query = query.Where(m =>
                    m.HContent.Contains(trimmedSearch) ||      // 搜尋訊息內容
                    m.HSenderRole.Contains(trimmedSearch) ||   // 搜尋發送者角色
                    m.HReceiverRole.Contains(trimmedSearch)    // 搜尋接收者角色
                );
            }

            // 計算總筆數和總頁數
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // 取得分頁後的資料
            var messages = await query
                .OrderByDescending(m => m.HTimestamp)  // 依時間降序排序
                .Skip((page - 1) * PageSize)          // 跳過前面頁數的資料
                .Take(PageSize)                       // 取得當前頁的資料
                .Include(m => m.HReports.Where(r => r.HStatus == "待處理"))  // 只包含待處理的檢舉
                .ToListAsync();

            // 獲取所有相關用戶的 ID
            var userIds = messages
                .SelectMany(m => new[] { m.HSenderId, m.HReceiverId })
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            // 獲取房客資訊
            var tenants = await _context.HTenants
                .Where(t => userIds.Contains(t.HTenantId))
                .ToDictionaryAsync(t => t.HTenantId, t => t.HUserName);

            // 獲取房東資訊
            var landlords = await _context.HLandlords
                .Where(l => userIds.Contains(l.HLandlordId))
                .ToDictionaryAsync(l => l.HLandlordId, l => l.HLandlordName);

            // 建立用戶名稱字典
            var userNames = new Dictionary<(int?, string), string>();
            foreach (var message in messages)
            {
                if (message.HSenderId.HasValue)
                {
                    var senderKey = (message.HSenderId, message.HSenderRole);
                    if (!userNames.ContainsKey(senderKey))
                    {
                        userNames[senderKey] = GetUserName(message.HSenderId, message.HSenderRole, tenants, landlords);
                    }
                }

                if (message.HReceiverId.HasValue)
                {
                    var receiverKey = (message.HReceiverId, message.HReceiverRole);
                    if (!userNames.ContainsKey(receiverKey))
                    {
                        userNames[receiverKey] = GetUserName(message.HReceiverId, message.HReceiverRole, tenants, landlords);
                    }
                }
            }

            // 設定ViewBag資料供視圖使用
            ViewBag.CurrentPage = page;               // 當前頁碼
            ViewBag.TotalPages = totalPages;          // 總頁數
            ViewBag.SearchString = searchString;      // 搜尋關鍵字
            ViewBag.UserNames = userNames;            // 用戶名稱字典

            // 返回視圖，並傳入訊息列表
            return View(messages);
        }

        [HttpPost]
        //防止跨站請求偽造攻擊
        [ValidateAntiForgeryToken]
        // async/await 非同步版delete寫法
        public async Task<IActionResult> Delete(int id)
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var message = await _context.HMessages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            // 將訊息標記為已刪除，而不是實際刪除
            message.HIsDeleted = 1;
            await _context.SaveChangesAsync();

            //刪除成功提示
            TempData["SuccessMessage"] = "刪除成功！";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("Admin/PrivateMessages/PrivateChat/{senderId}/{receiverId}")]
        public async Task<IActionResult> PrivateChat(int senderId, int receiverId, string senderRole = null, string receiverRole = null)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var messages = await _context.HMessages
                .Where(m =>
                    m.HChatId == null &&
                    ((m.HSenderId == senderId && m.HReceiverId == receiverId) ||
                     (m.HSenderId == receiverId && m.HReceiverId == senderId)))
                .OrderBy(m => m.HTimestamp)
                .ToListAsync();

            if (!messages.Any())
            {
                return View(new List<dynamic>());
            }

            // 如果外部沒傳入角色，才從第一條訊息推測
            if (senderRole == null || receiverRole == null)
            {
                var firstMessage = messages.First();
                if (firstMessage.HSenderId == senderId)
                {
                    senderRole = firstMessage.HSenderRole;
                    receiverRole = firstMessage.HReceiverRole;
                }
                else
                {
                    senderRole = firstMessage.HReceiverRole;
                    receiverRole = firstMessage.HSenderRole;
                }
            }

            var userIds = new[] { senderId, receiverId }.Distinct().ToList();

            var tenants = await _context.HTenants
                .Where(t => userIds.Contains(t.HTenantId))
                .ToDictionaryAsync(t => t.HTenantId, t => t.HUserName);

            var landlords = await _context.HLandlords
                .Where(l => userIds.Contains(l.HLandlordId))
                .ToDictionaryAsync(l => l.HLandlordId, l => l.HLandlordName);

            // 建立包含訊息和發送者名稱的動態對象列表
            var chatMessages = messages.Select(m => new
            {
                Message = m,
                SenderName = GetUserName(m.HSenderId, m.HSenderRole, tenants, landlords),
                ReceiverName = GetUserName(m.HReceiverId, m.HReceiverRole, tenants, landlords)
            }).ToList();

            ViewBag.SenderId = senderId;
            ViewBag.ReceiverId = receiverId;
            ViewBag.SenderRole = senderRole;
            ViewBag.ReceiverRole = receiverRole;

            return View(chatMessages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int messageId, string reason)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            var message = await _context.HMessages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            var report = new HReport
            {
                HMessageId = messageId,
                HAuthorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                HAuthorType = "Admin",
                HReason = reason,
                HStatus = "待處理",
                HCreatedAt = DateTime.Now,
                HReportType = "Private"
            };

            _context.HReports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "檢舉已成功提交！";

            // 這裡改掉！重新導向回私人聊天頁面，而不是列表
            if (message.HSenderId.HasValue && message.HReceiverId.HasValue)
            {
                return RedirectToAction("PrivateChat", new
                {
                    senderId = message.HSenderId.Value,
                    receiverId = message.HReceiverId.Value,
                    senderRole = message.HSenderRole,
                    receiverRole = message.HReceiverRole
                });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessReport(int reportId, string status)
        {
            try
            {
                // 檢查管理者權限
                if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                    return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

                // 檢查檢舉是否存在
                var report = await _context.HReports.FindAsync(reportId);
                if (report == null)
                {
                    return Json(new { success = false, message = "找不到此檢舉" });
                }

                // 從 Session 取管理員帳號
                var account = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
                if (string.IsNullOrEmpty(account))
                {
                    return Json(new { success = false, message = "登入狀態異常，請重新登入！" });
                }

                // 根據帳號找出 HAdmin
                var admin = await _context.HAdmins.FirstOrDefaultAsync(a => a.HAccount == account);
                if (admin == null)
                {
                    return Json(new { success = false, message = "登入狀態異常，請重新登入！" });
                }

                if (string.IsNullOrEmpty(status) || (status != "已核准" && status != "已拒絕"))
                {
                    return Json(new { success = false, message = "無效的狀態" });
                }

                // 更新檢舉資料
                report.HStatus = status;
                report.HReviewedAt = DateTime.Now;
                report.HAdminId = admin.HAdminId;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    var innerException = ex.InnerException?.Message ?? ex.Message;
                    return Json(new { success = false, message = $"資料庫更新失敗: {innerException}" });
                }

                return Json(new { success = true, message = "檢舉狀態已更新" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新失敗: {ex.Message}\n{ex.StackTrace}" });
            }
        }
        //檢舉列表頁面
        public async Task<IActionResult> ReportList()
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            //  獲取所有私人訊息的檢舉記錄
            var reports = await _context.HReports
                .Where(r => r.HReportType == "Private") // 只顯示私人訊息的檢舉
                .Include(r => r.HAdmin)                 // 包含處理的管理員資訊
                .Include(r => r.HMessage)               // 包含訊息資訊
                .OrderByDescending(r => r.HCreatedAt)
                .AsNoTracking()                        // 提高性能
                .ToListAsync();

            // 獲取所有相關的訊息
            var messageIds = reports.Select(r => r.HMessageId).Where(id => id.HasValue).Select(id => id.Value).ToList();
            var messages = await _context.HMessages
                .Where(m => messageIds.Contains(m.HMessageId))
                .AsNoTracking()
                .ToDictionaryAsync(m => m.HMessageId);

            // 將訊息資訊存儲在 ViewBag 中
            ViewBag.Messages = messages;

            return View(reports);
        }

        /// <summary>
        /// 獲取兩個用戶之間的對話記錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChatHistory(int senderId, int receiverId)
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 獲取房客資訊
            var tenants = await _context.HTenants
                .Where(t => t.HTenantId == senderId || t.HTenantId == receiverId)
                .ToDictionaryAsync(t => t.HTenantId, t => t.HUserName);

            // 獲取房東資訊（移除已驗證的限制）
            var landlords = await _context.HLandlords
                .Where(l => l.HLandlordId == senderId || l.HLandlordId == receiverId)
                .ToDictionaryAsync(l => l.HLandlordId, l => l.HLandlordName);

            // 獲取這兩個用戶之間的所有私人訊息
            var messages = await _context.HMessages
                .Where(m =>
                    m.HChatId == null && // 只查詢私人訊息
                    ((m.HSenderId == senderId && m.HReceiverId == receiverId) ||
                     (m.HSenderId == receiverId && m.HReceiverId == senderId))
                )
                .OrderBy(m => m.HTimestamp) // 按時間順序排序
                .Select(m => new
                {
                    m.HMessageId,
                    m.HSenderId,
                    m.HReceiverId,
                    m.HSenderRole,
                    m.HReceiverRole,
                    m.HContent,
                    m.HTimestamp,
                    m.HIsDeleted
                })
                .ToListAsync();

            // 為每條訊息添加發送者和接收者的名稱
            var result = messages.Select(m => new
            {
                m.HMessageId,
                m.HSenderId,
                m.HReceiverId,
                m.HContent,
                m.HTimestamp,
                m.HIsDeleted,
                SenderName = GetUserName(m.HSenderId, m.HSenderRole, tenants, landlords),
                ReceiverName = GetUserName(m.HReceiverId, m.HReceiverRole, tenants, landlords)
            });

            return Json(result);
        }

        /// <summary>
        /// 根據用戶ID和角色獲取用戶名稱
        /// </summary>
        private string GetUserName(int? userId, string role,
            Dictionary<int, string> tenants, Dictionary<int, string> landlords)
        {
            if (!userId.HasValue) return "系統";

            // 檢查是否為房客
            if (role?.ToLower() == "tenant")
            {
                if (tenants.ContainsKey(userId.Value))
                {
                    var name = tenants[userId.Value];
                    return !string.IsNullOrEmpty(name) ? name : $"房客 {userId}";
                }
                return $"房客 {userId}";
            }

            // 檢查是否為房東
            if (role?.ToLower() == "landlord")
            {
                if (landlords.ContainsKey(userId.Value))
                {
                    var name = landlords[userId.Value];
                    return !string.IsNullOrEmpty(name) ? name : $"房東 {userId}";
                }
                return $"房東 {userId}";
            }

            // 如果角色為空或無法識別，根據是否在房客或房東字典中來判斷
            if (tenants.ContainsKey(userId.Value))
            {
                var name = tenants[userId.Value];
                return !string.IsNullOrEmpty(name) ? name : $"房客 {userId}";
            }

            if (landlords.ContainsKey(userId.Value))
            {
                var name = landlords[userId.Value];
                return !string.IsNullOrEmpty(name) ? name : $"房東 {userId}";
            }

            return $"用戶 {userId}";
        }
        [HttpGet]
        [Route("Admin/PrivateMessages/PrivateChatByMessage/{messageId}")]
        public async Task<IActionResult> PrivateChatByMessage(int messageId)
        {
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 找到這則訊息
            var message = await _context.HMessages.FindAsync(messageId);
            if (message == null)
                return NotFound();

            int senderId = message.HSenderId ?? 0;
            int receiverId = message.HReceiverId ?? 0;
            string senderRole = message.HSenderRole;
            string receiverRole = message.HReceiverRole;

            // 直接呼叫更新後的 PrivateChat 方法
            return await PrivateChat(senderId, receiverId, senderRole, receiverRole);
        }

    }
}
