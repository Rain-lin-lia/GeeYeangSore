namespace GeeYeangSore.DTO.Commerce
{
    public class CreateAdRequest
    {
        public int PlanId { get; set; }  // 廣告方案
        public int PropertyId { get; set; }  // 哪一間房
        public string AdName { get; set; }  // 廣告名稱（例如：海景第一排）
        public string AdTag { get; set; }   // 選填：精選/置頂等
        public string TargetRegion { get; set; }  // 地區
        public string LinkURL { get; set; }  // 可點連結
        public string ImageURL { get; set; } // 首圖連結
    }
}
