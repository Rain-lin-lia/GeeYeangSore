using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GeeYeangSore.APIControllers.Contact;

[ApiController]
[Route("api/[controller]")]
public class ContactController : BaseController
{
    private readonly GeeYeangSoreContext _db;

    public ContactController(GeeYeangSoreContext db) : base(db)
    {
        _db = db;
    }

    [HttpPost("contact")]
    public async Task<IActionResult> CreateContact([FromBody] HContact data)
    {
        if (data == null)
        {
            return BadRequest("提交的數據不能為空");
        }

        try
        {
            // 高效地查詢資料庫
            var existingContact = await _db.HContacts
                .FirstOrDefaultAsync(e => e.HTenantId == data.HTenantId);

            if (existingContact != null)
            {
                data.HEmail = existingContact.HEmail;
            }

            data.HCreatedAt = DateTime.UtcNow;

            await _db.HContacts.AddAsync(data);
            await _db.SaveChangesAsync();

            return Ok(new { response = "資料庫寫入完成", data });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"創建聯絡人時出錯: {ex.Message}");
            return StatusCode(500, "處理請求時發生錯誤");
        }
    }
}