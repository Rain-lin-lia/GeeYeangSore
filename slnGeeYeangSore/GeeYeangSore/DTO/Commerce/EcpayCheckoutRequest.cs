namespace GeeYeangSore.DTO.Commerce
{
    public class EcpayCheckoutRequest
    {
        public int TotalAmount { get; set; }
        public string ItemName { get; set; }
        public int TenantId { get; set; }    // 建議從登入者取得
        public int AdId { get; set; }      // 廣告編號
    }
}
