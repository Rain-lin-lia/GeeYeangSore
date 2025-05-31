using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using GeeYeangSore.Models;
using GeeYeangSore.Hubs;
using GeeYeangSore.APIControllers.Session;

namespace GeeYeangSore.APIControllers.Chat
{
    [Route("api/chat")]
    [ApiController]
    public class ImageUploadController : BaseController
    {
        private readonly IHubContext<ChatHub> _hubContext;
        public ImageUploadController(GeeYeangSoreContext db, IHubContext<ChatHub> hubContext) : base(db)
        {
            _hubContext = hubContext;
        }

        public class UploadChatImageRequest
        {
            public IFormFile Image { get; set; }
            public int ReceiverId { get; set; }
            public string ReceiverRole { get; set; }
            public int? ChatId { get; set; }
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadChatImage([FromForm] UploadChatImageRequest request)
        {
            try
            {
                // 檢查是否已登入
                var access = CheckAccess();
                if (access != null) return access;
                var sender = GetCurrentTenant();

                var image = request.Image;
                var receiverId = request.ReceiverId;
                var receiverRole = request.ReceiverRole;
                var chatId = request.ChatId;

                // 檢查是否選擇檔案
                if (image == null || image.Length == 0)
                    return BadRequest(new { success = false, message = "未選擇檔案" });

                // 檢查是否為圖片格式
                if (!image.ContentType.StartsWith("image/"))
                    return BadRequest(new { success = false, message = "非圖片格式" });

                // 檢查檔案大小
                const long maxFileSize = 10 * 1024 * 1024;
                if (image.Length > maxFileSize)
                    return BadRequest(new { success = false, message = "檔案過大，請勿超過 10MB" });

                // 檢查圖片格式
                var extension = Path.GetExtension(image.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { success = false, message = "不支援的圖片格式" });

                // 儲存圖片
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/chat");
                Directory.CreateDirectory(uploads);

                // 生成唯一檔案名稱
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploads, fileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                // 生成圖片 URL 
                var imageUrl = $"/images/chat/{fileName}";

                // 寫入資料庫
                var receiver = _db.HTenants.FirstOrDefault(t => t.HTenantId == receiverId);
                var msg = new HMessage
                {
                    HSenderId = sender.HTenantId,
                    HReceiverId = receiverId,
                    HSenderRole = sender.HIsLandlord ? "landlord" : "tenant",
                    HReceiverRole = receiver != null && receiver.HIsLandlord ? "landlord" : "tenant",
                    HContent = imageUrl,
                    HMessageType = "image",
                    HIsRead = 0,
                    HSource = "私人",
                    HTimestamp = DateTime.Now,
                };
                _db.HMessages.Add(msg);
                await _db.SaveChangesAsync();

                // 廣播訊息給 receiver
                if (ChatHub.UserConnMap.TryGetValue(receiverId, out var connId) && !string.IsNullOrEmpty(connId))
                {
                    await _hubContext.Clients.Client(connId).SendAsync("ReceiveMessage", new
                    {
                        id = msg.HMessageId,
                        from = msg.HSenderId,
                        to = msg.HReceiverId,
                        type = msg.HMessageType,
                        content = msg.HContent,
                        time = msg.HTimestamp?.ToString("HH:mm:ss")
                    });
                }
                // 廣播訊息給 sender（自己）
                if (ChatHub.UserConnMap.TryGetValue(sender.HTenantId, out var selfConnId) && !string.IsNullOrEmpty(selfConnId))
                {
                    await _hubContext.Clients.Client(selfConnId).SendAsync("ReceiveMessage", new
                    {
                        id = msg.HMessageId,
                        from = msg.HSenderId,
                        to = msg.HReceiverId,
                        type = msg.HMessageType,
                        content = msg.HContent,
                        time = msg.HTimestamp?.ToString("HH:mm:ss")
                    });
                }

                return Ok(new { success = true, message = "上傳成功", imageUrl, messageId = msg.HMessageId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "圖片上傳失敗", error = ex.Message });
            }
        }
    }
}