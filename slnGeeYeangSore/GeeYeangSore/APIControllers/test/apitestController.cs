using Microsoft.AspNetCore.Mvc;
using GeeYeangSore.Models;
using GeeYeangSore.Data;

namespace GeeYeangSore.APIControllers.test
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly GeeYeangSoreContext _db;
        public TestController(GeeYeangSoreContext db)
        {
            _db = db;
        }

        // 讀取所有 tenant（只回傳指定欄位）
        [HttpGet]
        public IActionResult GetAll()
        {
            var tenants = _db.HTenants
                .Select(t => new
                {
                    t.HTenantId,
                    t.HUserName,
                    t.HBirthday,
                    t.HGender,
                    t.HAddress
                }).ToList();
            return Ok(tenants);
        }

        // 讀取單一 tenant（只回傳指定欄位）
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var tenant = _db.HTenants
                .Where(t => t.HTenantId == id)
                .Select(t => new
                {
                    t.HTenantId,
                    t.HUserName,
                    t.HBirthday,
                    t.HGender,
                    t.HAddress
                })
                .FirstOrDefault();
            if (tenant == null)
                return NotFound("找不到資料");
            return Ok(tenant);
        }

        // -------------------- HMessage --------------------

        // 讀取所有 message（只回傳指定欄位）
        [HttpGet("messages")]
        public IActionResult GetAllMessages()
        {
            var messages = _db.HMessages
                .Select(m => new
                {
                    m.HMessageId,
                    m.HChatId,
                    m.HSenderId,
                    m.HReceiverId,
                    m.HContent,
                    m.HTimestamp
                }).ToList();
            return Ok(messages);
        }

        // 讀取單一 message（只回傳指定欄位）
        [HttpGet("messages/{id}")]
        public IActionResult GetMessageById(int id)
        {
            var message = _db.HMessages
                .Where(m => m.HMessageId == id)
                .Select(m => new
                {
                    m.HMessageId,
                    m.HChatId,
                    m.HSenderId,
                    m.HReceiverId,
                    m.HContent,
                    m.HTimestamp
                })
                .FirstOrDefault();
            if (message == null)
                return NotFound("找不到資料");
            return Ok(message);
        }
    }
}

