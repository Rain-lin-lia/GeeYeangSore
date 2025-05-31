namespace GeeYeangSore.DTO.User
{

    //驗證使用者輸入的驗證碼是否正確
    public class VerifyTokenDto
    {
        public string UserEmail { get; set; }     // 使用者輸入的信箱
        public string InputToken { get; set; }    // 使用者輸入的驗證碼
    }
}
