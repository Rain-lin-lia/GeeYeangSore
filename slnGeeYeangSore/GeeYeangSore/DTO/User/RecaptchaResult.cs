namespace GeeYeangSore.DTO.User
{
    public class RecaptchaResult //接收Recaptchav3使用
    {
        public bool Success { get; set; }
        public float Score { get; set; }
        public string Action { get; set; }
        public DateTime Challenge_ts { get; set; }
        public string Hostname { get; set; }
    }
}
