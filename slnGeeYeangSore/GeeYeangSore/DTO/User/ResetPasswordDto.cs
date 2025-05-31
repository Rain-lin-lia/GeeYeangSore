public class ResetPasswordDto //使用者完成驗證後「重設密碼」
{
    public string Email { get; set; }
    public string Code { get; set; }
    public string NewPassword { get; set; }
}