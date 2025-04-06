using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;
using GeeYeangSore.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                .ToListAsync();

            // 設定ViewBag資料供視圖使用
            ViewBag.CurrentPage = page;               // 當前頁碼
            ViewBag.TotalPages = totalPages;          // 總頁數
            ViewBag.SearchString = searchString;      // 搜尋關鍵字

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
            //記錄「我要刪除這筆」，還沒有真的刪除到資料庫
            _context.HMessages.Remove(message);
            //把剛剛註冊的刪除動作，真正同步到資料庫
            await _context.SaveChangesAsync();

            //刪除成功提示
            TempData["SuccessMessage"] = "刪除成功！";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("Admin/PrivateMessages/PrivateChat/{senderId}/{receiverId}")]
        public async Task<IActionResult> PrivateChat(int senderId, int receiverId)
        {
            // 檢查管理者權限
            if (!HasAnyRole("超級管理員", "系統管理員", "內容管理員"))
                return RedirectToAction("NoPermission", "Home", new { area = "Admin" });

            // 獲取特定發送者和接收者之間的私人訊息（HChatId 為 null）
            var messages = await _context.HMessages
                .Where(m =>
                    m.HChatId == null && // 只顯示私人對話
                     m.HSenderId != null &&  // 發送者存在
                     m.HReceiverId != null && m.HReceiverId != 0 &&    // 接收者存在
                    ((m.HSenderId == senderId && m.HReceiverId == receiverId) ||
                    (m.HSenderId == receiverId && m.HReceiverId == senderId)))
                .OrderBy(m => m.HTimestamp)
                .ToListAsync();

            // 設定 ViewBag 資料
            ViewBag.SenderId = senderId;
            ViewBag.ReceiverId = receiverId;
            ViewBag.MessageCount = messages.Count;

            return View(messages);
        }

    }
}
