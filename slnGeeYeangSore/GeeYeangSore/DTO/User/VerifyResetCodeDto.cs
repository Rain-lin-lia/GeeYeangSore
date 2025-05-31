public class VerifyResetCodeDto //使用者輸入驗證碼進行「身份驗證」
{
    public string Email { get; set; }
    public string Code { get; set; }
}