using System.ComponentModel.DataAnnotations;

namespace GeeYeangSore.ViewModels
{
    public class CLoginViewModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        public string txtAccount { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        public string txtPassword { get; set; }

        // 🆕 新增 reCAPTCHA Token 欄位
        [Required(ErrorMessage = "reCAPTCHA 驗證失敗，請重新操作")]
        public string RecaptchaToken { get; set; }

    }
}