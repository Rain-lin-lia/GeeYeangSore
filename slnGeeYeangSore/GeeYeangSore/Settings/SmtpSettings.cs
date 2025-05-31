namespace GeeYeangSore.Settings
{
    public class SmtpSettings //用來透過 .NET 的 IOptions<SmtpSettings> 自動綁定設定檔
    {
        public string FromEmail { get; set; }       // 寄件者信箱（Gmail）
        public string AppPassword { get; set; }     // 應用程式密碼（16 碼）
        public string Host { get; set; }            // SMTP 主機（ex: smtp.gmail.com）
        public int Port { get; set; }               // SMTP 埠號（ex: 587）
    }
}
