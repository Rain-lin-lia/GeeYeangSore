using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.APIControllers.test
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionTestController : ControllerBase
    {
        /// <summary>
        /// 測試後端是否啟動成功。
        /// </summary>
        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok(new { message = "後端 API 正常運作！" });
        }

        /// <summary>
        /// 將測試資料寫入 Session（Key: TestKey）。
        /// </summary>
        [HttpPost("set-session")]
        public IActionResult SetSession()
        {
            HttpContext.Session.SetString("TestKey", "HelloSession");
            return Ok(new { message = "Session 寫入成功（TestKey=HelloSession）" });
        }

        /// <summary>
        /// 讀取目前 Session 中的 TestKey 值。
        /// </summary>
        [HttpGet("get-session")]
        public IActionResult GetSession()
        {
            var value = HttpContext.Session.GetString("TestKey");
            if (string.IsNullOrEmpty(value))
                return Ok(new { message = "尚未設定 Session 或已過期。" });

            return Ok(new { message = $"目前 Session TestKey 值為：{value}" });
        }
    }
}
