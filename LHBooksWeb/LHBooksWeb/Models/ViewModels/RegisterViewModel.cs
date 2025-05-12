using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.ViewModels
{
    public class RegisterViewModel
    {
		[Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
		[Display(Name = "Họ và tên")]
        public string FullName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
		[Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập Email.")]
		[EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập sđt.")]
		[Phone]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$", ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, chứa ít nhất một chữ cái viết thường, một chữ cái viết hoa và một ký tự đặc biệt.")]
        public string Password { get; set; }



        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
		[DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }
    }
}
