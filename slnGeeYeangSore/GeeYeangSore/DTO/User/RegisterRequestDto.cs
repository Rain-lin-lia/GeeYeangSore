namespace GeeYeangSore.DTO.User
{
    public class RegisterRequestDto
    {
        public string UserName { get; set; }              // 姓名
        public string Phone { get; set; }                 // 手機號碼
        public string Email { get; set; }                 // 註冊用 Email
        public string Password { get; set; }              // 密碼
        public bool IsAgreePolicy { get; set; }           // 是否已勾選同意隱私條款
        public string VerificationCode { get; set; }      // 前端填入的驗證碼
    }
}
