using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
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
        public ICollection<SelectListItem> Providers { get; set; } // Sử dụng Microsoft.AspNetCore.Mvc.Rendering
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập?")]
        public bool RememberMe { get; set; }

    }

    public class UserProfileViewModel
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string UserName { get; set; }

        public string Address { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập sđt.")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại chỉ được chứa chữ số và có độ dài 10–11 số.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
        public string UserName { get; set; } // Không cho chỉnh sửa tên đăng nhập

        public string Address { get; set; } // Địa chỉ đầy đủ

        [Required(ErrorMessage = "Tỉnh/Thành không được để trống")]
        public string Province { get; set; }  // Tỉnh/Thành phố

        [Required(ErrorMessage = "Quận/Huyện không được để trống")]
        public string District { get; set; }  // Quận/Huyện

        [Required(ErrorMessage = "Phường/Xã không được để trống")]
        public string Ward { get; set; }      // Xã/Phường

        [Required(ErrorMessage = "Địa chỉ cụ thể không được để trống")]
        public string SpecificAddress { get; set; } // Địa chỉ cụ thể
    }


    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]

        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quyền.")]
        [Display(Name = "Quyền")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$", ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, chứa ít nhất một chữ cái viết thường, một chữ cái viết hoa và một ký tự đặc biệt.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
        [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khẩu.")]
        public string ConfirmPassword { get; set; }
    }


    public class EditAccountViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }


    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$", ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, chứa ít nhất một chữ cái viết thường, một chữ cái viết hoa và một ký tự đặc biệt.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }

    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$", ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, chứa ít nhất một chữ cái viết thường, một chữ cái viết hoa và một ký tự đặc biệt.")]

        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserDetailViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; } // Giả sử bạn có trường này thêm vào IdentityUser
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public string isActive { get; set; }
    }


}
