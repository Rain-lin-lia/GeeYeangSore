using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NoticeController : ControllerBase
{
    private readonly GeeYeangSoreContext _db;
    private readonly IWebHostEnvironment _env;

    public NoticeController(GeeYeangSoreContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("news")]
    public IActionResult GetNews()
    {
        var news = _db.HNews.ToList();

        foreach (var item in news)
        {
            if (!string.IsNullOrEmpty(item.HImagePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, item.HImagePath.TrimStart('/'));

                if (System.IO.File.Exists(filePath))
                {
                    var imageBytes = System.IO.File.ReadAllBytes(filePath);
                    var base64Image = Convert.ToBase64String(imageBytes);

                    // 將圖片轉換成 Base64 存入一個新的屬性
                    item.HImagePath = base64Image;
                }
            }
        }

        return Ok(new { response = news });
    }
}