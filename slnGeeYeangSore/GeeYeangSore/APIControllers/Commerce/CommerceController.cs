using GeeYeangSore.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.APIControllers.Commerce
{
    [Route("api/commerce")]
    [ApiController]
    public class CommerceController : BaseController
    {
        private readonly string _ngrokBaseUrl;

        public CommerceController(GeeYeangSoreContext db, IConfiguration config) : base(db)
        {
            _ngrokBaseUrl = config["NgrokBaseUrl"];
        }

        // 根據方案ID回傳金額及名稱
        [HttpGet("plan-info/{planId}")]
        public IActionResult GetPlanInfo(int planId)
        {
            try
            {
                // 步驟1：查詢指定方案
                var plan = _db.HAdPlans.FirstOrDefault(p => p.HPlanId == planId);
                if (plan == null)
                    return NotFound(new { success = false, message = "查無此方案" });

                // 步驟2：回傳方案資訊
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        planId = plan.HPlanId,
                        name = plan.HName,
                        category = plan.HCategory,
                        price = plan.HAdPrice,
                        days = plan.HDays
                    }
                });
            }
            catch (Exception ex)
            {
                // 步驟3：例外處理
                return StatusCode(500, new { success = false, message = "伺服器錯誤", error = ex.Message });
            }
        }
        [HttpPost("checkout-params")]
        public IActionResult GenerateEcpayParams([FromBody] GeeYeangSore.DTO.Commerce.EcpayCheckoutRequest vm)
        {
            try
            {
                // 步驟1：取得目前登入的租客
                var tenant = GetCurrentTenant();
                if (tenant == null)
                    return Unauthorized(new { success = false, message = "未登入" });

                // 步驟2：產生訂單編號與時間
                // 訂單編號（你送給綠界的唯一編號，用於 h_Merchant_Trade_No）
                // 例如：M202505200001
                string orderId = $"M{DateTime.Now:yyyyMMddHHmmssfff}";

                // 建立時間（符合綠界格式 yyyy/MM/dd HH:mm:ss）
                string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                // 統一使用設定檔 ngrok 網址
                string website = _ngrokBaseUrl;

                // 步驟3：組成綠界訂單參數
                var order = new Dictionary<string, string>
                {
                    // 商店代號（測試用為 2000132，正式請換為你平台專屬代號）
                    { "MerchantID", "2000132" },

                    // 訂單編號，必須唯一且不得超過 20 字元（英數字）
                    { "MerchantTradeNo", orderId },

                    // 訂單建立時間，格式必須是 yyyy/MM/dd HH:mm:ss（24 小時制）
                    { "MerchantTradeDate", now }, // e.g., "2025/05/19 15:45:30"

                    // 固定為 aio，表示「全功能支付」
                    { "PaymentType", "aio" },

                    // 訂單總金額，必須是整數（單位：新台幣）
                    { "TotalAmount", vm.TotalAmount.ToString() },

                    // 商品描述（建議使用 URL encode 處理中文）
                    { "TradeDesc", Uri.EscapeDataString("居研所廣告刊登付款") },

                    // 商品名稱（可包含中文，複數商品用 # 分隔，限 200 字元）
                    { "ItemName", vm.ItemName },

                    // 綠界付款完成通知的接收網址（必須可由綠界主機 POST 存取）
                    { "ReturnURL", $"{website}/api/commerce/ecpay/callback" },

                    // 用戶完成付款後導回的網址（前端接收處）
                    { "ClientBackURL", $"http://localhost:5173/frontend/ad-confirm/{orderId}" },

                    // 指定付款方式（此處為信用卡一次付清）
                    { "ChoosePayment", "Credit" },

                    // 自訂欄位 1，可用來記錄租客 ID（回傳時會帶回）
                    { "CustomField1", tenant.HTenantId.ToString() },

                    // 自訂欄位 2，可用來記錄廣告 ID 或方案 ID
                    { "CustomField2", vm.AdId.ToString() },

                    // 使用加密版本：1 表示使用 SHA256 驗證（官方建議值）
                    { "EncryptType", "1" }
                };

                // 步驟4：產生加密驗證碼（CheckMacValue）
                order["CheckMacValue"] = GenerateCheckMacValue(order);

                // 步驟5：回傳綠界訂單參數（前端可用於自行組表單）
                return Ok(new { success = true, data = order });
            }
            catch (Exception ex)
            {
                // 步驟6：例外處理
                return StatusCode(500, new { success = false, message = "綠界參數生成失敗", error = ex.Message });
            }
        }

        // 產生自動送出 HTML 表單 API
        [HttpPost("ecpay-html")]
        public IActionResult GenerateEcpayHtml([FromBody] GeeYeangSore.DTO.Commerce.EcpayCheckoutRequest vm)
        {
            // 步驟1：驗證登入
            var tenant = GetCurrentTenant();
            if (tenant == null)
                return Unauthorized(new { success = false, message = "未登入" });

            // 步驟2：組合訂單參數（同 checkout-params）
            string orderId = $"M{DateTime.Now:yyyyMMddHHmmssfff}";
            string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string website = _ngrokBaseUrl;
            var order = new Dictionary<string, string>
            {
                { "MerchantID", "2000132" },
                { "MerchantTradeNo", orderId },
                { "MerchantTradeDate", now },
                { "PaymentType", "aio" },
                { "TotalAmount", vm.TotalAmount.ToString() },
                { "TradeDesc", Uri.EscapeDataString("居研所廣告刊登付款") },
                { "ItemName", vm.ItemName },
                { "ReturnURL", $"{website}/api/commerce/ecpay/callback" },
                { "ClientBackURL", $"http://localhost:5173/frontend/ad-confirm/{orderId}" },
                { "ChoosePayment", "Credit" },
                { "CustomField1", tenant.HTenantId.ToString() },
                { "CustomField2", vm.AdId.ToString() },
                { "EncryptType", "1" }
            };
            // 步驟3：產生自動送出 HTML 表單
            string html = GenerateAutoSubmitForm(order, true, true);
            // 步驟4：回傳 HTML 給前端，前端插入後自動跳轉到綠界
            return Content(html, "text/html");
        }

        // 產生自動送出 HTML 表單的工具方法
        private string GenerateAutoSubmitForm(Dictionary<string, string> orderData, bool includeScript = true, bool useStage = true)
        {
            // 加入 CheckMacValue
            var checkMac = GenerateCheckMacValue(orderData);
            orderData["CheckMacValue"] = checkMac;
            var form = new StringBuilder();
            string actionUrl = useStage
                ? "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5"
                : "https://payment.ecpay.com.tw/Cashier/AioCheckOut/V5";
            form.Append($"<form id='ecpay-form' method='post' action='{actionUrl}'>");
            foreach (var kv in orderData)
            {
                var key = System.Web.HttpUtility.HtmlEncode(kv.Key);
                var value = System.Web.HttpUtility.HtmlEncode(kv.Value);
                form.Append($"<input type='hidden' name='{key}' value='{value}' />");
            }
            form.Append("</form>");
            if (includeScript)
                form.Append("<script>document.getElementById('ecpay-form').submit();</script>");
            return form.ToString();
        }

        // 優化版 CheckMacValue 產生（參考 EcpayHelper）
        private string GenerateCheckMacValue(Dictionary<string, string> parameters)
        {
            const string HashKey = "5294y06JbISpM5x9";
            const string HashIV = "v77hoKGq4kWxNNIS";
            var sorted = parameters
                .Where(kv => kv.Key != "CheckMacValue")
                .OrderBy(kv => kv.Key)
                .ToList();
            var raw = $"HashKey={HashKey}&{string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"))}&HashIV={HashIV}";
            raw = System.Web.HttpUtility.UrlEncode(raw).ToLower();
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }

        // 綠界付款完成 Callback
        [HttpPost("ecpay/callback")]
        public IActionResult EcpayCallback([FromForm] IFormCollection form)
        {
            var logPath = "D:/callback.log";
            var now = DateTime.Now;
            var logPrefix = $"[{now:yyyy-MM-dd HH:mm:ss}]";

            try
            {
                // Step 1: Log 原始綠界回傳資料
                System.IO.File.AppendAllText(logPath, $"{logPrefix} [Callback Received] {JsonConvert.SerializeObject(form)}\n");

                // Step 2: 解析欄位
                var rtnCode = form["RtnCode"].ToString(); // 1 代表成功
                var adIdStr = form["CustomField2"].ToString();

                if (!int.TryParse(adIdStr, out int adId))
                {
                    System.IO.File.AppendAllText(logPath, $"{logPrefix} [Error] 無法解析 AdId: {adIdStr}\n");
                    return Content("0|FAIL");
                }

                var ad = _db.HAds.FirstOrDefault(a => a.HAdId == adId);
                if (ad == null)
                {
                    System.IO.File.AppendAllText(logPath, $"{logPrefix} [Error] 找不到 AdId = {adId}\n");
                    return Content("0|FAIL");
                }

                // Step 3: 更新廣告資料
                if (rtnCode == "1")
                {
                    // 成功付款
                    int planDays = 30;
                    decimal adPrice = 0;
                    string adTag = "無";
                    int priority = 1;

                    var plan = _db.HAdPlans.FirstOrDefault(p => p.HPlanId == ad.HPlanId);
                    if (plan != null)
                    {
                        planDays = plan.HDays > 0 ? plan.HDays : 30;
                        adPrice = plan.HAdPrice;
                        priority = plan.HPlanId;
                        adTag = plan.HPlanId switch
                        {
                            2 => "推薦",
                            3 => "精選",
                            _ => "無"
                        };
                    }

                    var property = _db.HProperties.FirstOrDefault(p => p.HPropertyId == ad.HPropertyId);
                    string region = property?.HCity ?? "";
                    string description = property?.HDescription ?? "";

                    ad.HStatus = "已付款";
                    ad.HStartDate = now;
                    ad.HEndDate = now.AddDays(planDays);
                    ad.HLastUpdated = now;
                    ad.HAdPrice = adPrice;
                    ad.HIsDelete = false;
                    ad.HTargetRegion = region;
                    ad.HAdTag = adTag;
                    ad.HPriority = priority;
                    ad.HDescription = description;
                }
                else
                {
                    // 付款失敗或取消
                    ad.HStatus = "未付款";
                    ad.HLastUpdated = now;
                }

                //  明確告知 EF 這筆資料需要更新
                _db.Entry(ad).State = EntityState.Modified;

                // Step 4: 建立交易紀錄
                string paymentType = form["PaymentType"].ToString();
                string paymentTypeDisplay = paymentType == "Credit_CreditCard" ? "信用卡" : paymentType;
                string rtnMsg = form["RtnMsg"].ToString();
                string rtnMsgDisplay = rtnMsg == "交易成功" ? "付款成功" : rtnMsg;
                string itemName = ad.HAdName ?? "";

                var transaction = new HTransaction
                {
                    HMerchantTradeNo = form["MerchantTradeNo"],
                    HTradeNo = form["TradeNo"],
                    HAmount = decimal.TryParse(form["TradeAmt"], out var amt) ? amt : 0,
                    HItemName = itemName,
                    HPaymentType = paymentTypeDisplay,
                    HPaymentDate = DateTime.TryParse(form["PaymentDate"], out var payDate) ? payDate : now,
                    HTradeStatus = rtnCode == "1" ? "Success" : "Failed",
                    HRtnMsg = rtnMsgDisplay,
                    HIsSimulated = form["SimulatePaid"] == "1" ? 1 : 0,
                    HCreateTime = now,
                    HUpdateTime = now,
                    HRawJson = JsonConvert.SerializeObject(form),
                    HPropertyId = ad.HPropertyId,
                    HRegion = ad.HTargetRegion,
                    HAdId = ad.HAdId
                };

                _db.HTransactions.Add(transaction);
                _db.SaveChanges();

                System.IO.File.AppendAllText(logPath, $"{logPrefix} [Success] AdId = {ad.HAdId}, Status = {ad.HStatus}\n");
                return Content("1|OK");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(logPath, $"{logPrefix} [Exception] {ex.Message}\n");
                return Content("0|FAIL");
            }
        }


        private string GetSHA256(string value)
        {
            // 步驟1：建立 SHA256 實例
            var result = new StringBuilder();
            using (var sha256 = SHA256.Create())
            {
                var bts = Encoding.UTF8.GetBytes(value);
                var hash = sha256.ComputeHash(bts);
                // 步驟2：轉換為十六進位字串
                for (int i = 0; i < hash.Length; i++)
                {
                    result.Append(hash[i].ToString("X2"));
                }
            }
            // 步驟3：回傳結果
            return result.ToString();
        }
        [HttpGet("query-status/{orderId}")]
        public IActionResult QueryPaymentStatus(string orderId)
        {
            // 取得交易紀錄
            var tx = _db.HTransactions.FirstOrDefault(t => t.HMerchantTradeNo == orderId);
            if (tx == null)
                return NotFound(new { success = false, message = "查無交易" });

            // 預設方案天數
            int? planDays = null;

            // 若有關聯到廣告，再查出方案天數
            if (tx.HAdId.HasValue)
            {
                planDays = (from ad in _db.HAds
                            join plan in _db.HAdPlans on ad.HPlanId equals plan.HPlanId
                            where ad.HAdId == tx.HAdId.Value
                            select plan.HDays).FirstOrDefault();
            }
            return Ok(new
            {
                success = tx.HRtnMsg == "付款成功",
                orderId = tx.HMerchantTradeNo,
                itemName = tx.HItemName,
                amount = tx.HAmount,
                days = planDays ?? 0,
                paymentDate = tx.HPaymentDate?.ToString("yyyy-MM-dd HH:mm")
            });
        }
    }
}
