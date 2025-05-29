using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using GeeYeangSore.Models;
using System.Collections.Concurrent;

namespace GeeYeangSore.Hubs
{
    public class ChatHub : Hub
    {
        private readonly GeeYeangSoreContext _db;

        // key: HTenantId, value: connectionId（單一裝置登入）
        public static ConcurrentDictionary<int, string> UserConnMap = new();

        public ChatHub(GeeYeangSoreContext db)
        {
            _db = db;
        }

        private HTenant? GetSessionUser()
        {
            var http = Context.GetHttpContext();
            var email = http?.Session.GetString("SK_LOGINED_USER");
            if (string.IsNullOrEmpty(email)) return null;
            return _db.HTenants.FirstOrDefault(t => t.HEmail == email && !t.HIsDeleted);
        }

        /// <summary>
        /// 連線事件，驗證 Session 是否已登入
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var user = GetSessionUser();
                if (user == null)
                {
                    Context.Abort();
                    return;
                }
                // 單一裝置登入：重複登入自動覆蓋舊的 ConnectionId
                UserConnMap[user.HTenantId] = Context.ConnectionId;
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", "連線錯誤，請重新整理頁面。");
            }
        }

        /// <summary>
        /// 離線事件
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            try
            {
                var connId = Context.ConnectionId;
                var userId = UserConnMap.FirstOrDefault(x => x.Value == connId).Key;
                if (userId != 0)
                    UserConnMap.TryRemove(userId, out _);

                await base.OnDisconnectedAsync(ex);
            }
            catch (Exception ex2)
            {
                // 記錄 log
            }
        }

        /// <summary>
        /// 發送訊息（僅支援一對一），未登入不處理，並寫入資料庫
        /// </summary>
        /// <param name="fromId">發送者ID</param>
        /// <param name="toId">接收者ID</param>
        /// <param name="text">訊息內容</param>
        /// <param name="senderRole">發送者角色</param>
        /// <param name="receiverRole">接收者角色</param>
        /// <param name="messageType">訊息型別</param>
        /// <param name="source">來源</param>
        public async Task SendMessage(string fromId, string toId, string text)
        {
            try
            {
                var user = GetSessionUser();
                if (user == null)
                {
                    await Clients.Caller.SendAsync("ReceiveError", "尚未登入");
                    return;
                }
                if (!int.TryParse(fromId, out var f) || !int.TryParse(toId, out var t))
                {
                    await Clients.Caller.SendAsync("ReceiveError", "ID 格式錯誤");
                    return;
                }
                // 取得發送者身分訊息只會在房東與房客之間傳遞
                var sender = user;
                string senderRole = sender.HIsLandlord ? "landlord" : "tenant";
                string receiverRole = senderRole == "landlord" ? "tenant" : "landlord";
                var msg = new HMessage
                {
                    HSenderId = f,
                    HSenderRole = senderRole,
                    HReceiverId = t,
                    HReceiverRole = receiverRole,
                    HMessageType = "文字",
                    HContent = text,
                    HIsRead = 0,
                    HSource = "私人",
                    HTimestamp = DateTime.Now
                };
                // 新增訊息
                _db.HMessages.Add(msg);
                await _db.SaveChangesAsync();
                await _db.Entry(msg).ReloadAsync(); // 確保讀取自動產生的 ID
                                                    // 建立完整訊息物件
                var msgObj = new
                {
                    id = msg.HMessageId,
                    from = msg.HSenderId,
                    to = msg.HReceiverId,
                    content = msg.HContent,
                    senderRole = msg.HSenderRole,
                    receiverRole = msg.HReceiverRole,
                    messageType = msg.HMessageType,
                    isRead = msg.HIsRead,
                    source = msg.HSource,
                    time = msg.HTimestamp != null ? msg.HTimestamp.Value.ToString("HH:mm") : ""
                };
                await Clients.Caller.SendAsync("ReceiveMessage", msgObj);
                var receiverConnId = ChatHub.UserConnMap.GetValueOrDefault(msg.HReceiverId ?? 0);
                if (!string.IsNullOrEmpty(receiverConnId))
                {
                    await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", msgObj);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", $"伺服器錯誤：{ex.Message}");
            }
        }
    }
}