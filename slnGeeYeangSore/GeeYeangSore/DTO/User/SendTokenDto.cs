using System.ComponentModel.DataAnnotations;

//送出驗證碼（寄信）
public class SendTokenDto 
{
    [Required]
    [EmailAddress]
    public string UserEmail { get; set; }

    //public string Device { get; set; } //未來擴充（裝置辨識、記錄來源）預留空間
}
