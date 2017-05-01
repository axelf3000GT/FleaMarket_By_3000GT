using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleaMarket.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Поле \"Email\" должно быть заполнено")]
        [Display(Name = "Email")] 
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = "Поле \"Провайдер\" должно быть заполнено")]
        public string Provider { get; set; }

        [Required(ErrorMessage = "Поле Код должно быть заполнено")]
        [Display(Name = "Код")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Запомнить браузер?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessage = "Поле \"Email\" должно быть заполнено")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        internal object UserName;

        [Required(ErrorMessage = "Поле \"Email\" должно быть заполнено")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" должно быть заполнено")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Поле \"Email\" должно быть заполнено")] 
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" должно быть заполнено")]
        [StringLength(100, ErrorMessage = "Слишком короткий пароль, должно быть минимум 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и проверочный пароль не совпадают")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" должно быть заполнено")]
        [StringLength(100, ErrorMessage = "Слишком короткий пароль, должно быть минимум 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и проверочный пароль не совпадают")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Поле \"Email\" должно быть заполнено")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
