using LHBooksWeb.Data;
using LHBooksWeb.Models.ViewModels;
using LHBooksWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using LHBooksWeb.Services.Email;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Spreadsheet;
namespace LHBooksWeb.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IProvinceService _provinceService;
        private readonly Services.Email.IEmailSender _emailSender;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IProvinceService provinceService, LHBooksWeb.Services.Email.IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _provinceService = provinceService;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);


            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                ViewBag.IsLockedMessage = "Tài khoản không tồn tại.";
                return View(model);
            }

            // Check if user is active
            if (!user.isActive)
            {
                ViewBag.IsLockedMessage = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.";
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Đăng nhập không thành công. Tài khoản hoặc mật khẩu không chính xác!");
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Kiểm tra email đã tồn tại
            var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmailUser != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            // Kiểm tra tên người dùng đã tồn tại
            var existingUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserName != null)
            {
                ModelState.AddModelError("UserName", "Tên người dùng đã tồn tại.");
            }

            // Kiểm tra số điện thoại đã tồn tại
            var existingPhoneUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == model.Phone);

            if (existingPhoneUser != null)
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã được sử dụng.");
            }

            // Nếu có lỗi thì trả về view với thông báo lỗi
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.Phone,
                FullName = model.FullName,
                EmailConfirmed = true,
                Address = "Chưa có địa chỉ",
                isActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Gán quyền "Customer" nếu chưa có
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }

                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Tạo tài khoản thành công!";
                return RedirectToAction("Index", "Home");
            }

            // Thêm lỗi từ Identity nếu có
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }




        //lay tinh thanh

        [HttpGet]
        public IActionResult GetProvinces()
        {
            var provinces = _provinceService.GetProvinces()
                .Select(p => new { p.Code, p.Name })
                .ToList();

            return Ok(provinces);
        }

        [HttpGet]
        public IActionResult GetDistricts(string provinceName)
        {
            var province = _provinceService.GetProvinces()
                .FirstOrDefault(p => p.Name == provinceName);

            if (province != null)
            {
                var districts = province.Districts
                    .Select(d => new { d.Code, d.Name })
                    .ToList();

                return Ok(districts);
            }

            return Ok(new List<object>());
        }

        [HttpGet]
        public IActionResult GetWards(string districtName)
        {
            var district = _provinceService.GetProvinces()
                .SelectMany(p => p.Districts)
                .FirstOrDefault(d => d.Name == districtName);

            if (district != null)
            {
                var wards = district.Wards
                    .Select(w => new { w.Code, w.Name })
                    .ToList();

                return Ok(wards);
            }

            return Ok(new List<object>());
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            var userId = _userManager.GetUserId(User); // Lấy ID từ ClaimsPrincipal
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                UserName = user.UserName,
                Address = user.Address
            };

            return View(model);
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> EditProfile(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                HttpContext.Session.SetString("PreviousUrl", returnUrl);
            }
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }
            var addressParts = user.Address.Split(',');
            string specificAddress = addressParts[0].Trim();
            string ward = addressParts.Length > 1 ? addressParts[1].Trim() : string.Empty;
            string district = addressParts.Length > 2 ? addressParts[2].Trim() : string.Empty;
            string province = addressParts.Length > 3 ? addressParts[3].Trim() : string.Empty;


            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                UserName = user.UserName,
                Address = user.Address,
                SpecificAddress = specificAddress,
                Ward = ward,
                District = district,
                Province = province
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.SpecificAddress) ||
        string.IsNullOrWhiteSpace(model.Ward) ||
        string.IsNullOrWhiteSpace(model.District) ||
        string.IsNullOrWhiteSpace(model.Province))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin địa chỉ (Số nhà, Phường/Xã, Quận/Huyện, Tỉnh/Thành).");
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin cơ bản
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.Phone;

            // Ghép lại địa chỉ đầy đủ nếu có thay đổi
            string newAddress = $"{model.SpecificAddress}, {model.Ward}, {model.District}, {model.Province}";

            if (!string.Equals(user.Address, newAddress, StringComparison.OrdinalIgnoreCase))
            {
                user.Address = newAddress;
            }

            var result = await _userManager.UpdateAsync(user);
            var previousUrl = HttpContext.Session.GetString("PreviousUrl");
            if (result.Succeeded)
            {
                TempData["Success"] = "Cập nhật thônh tin thành công!";
                if (!string.IsNullOrEmpty(previousUrl))
                {
                    return Redirect(previousUrl);
                }

                return RedirectToAction("UserProfile");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }



        public IActionResult AccessDenied()
        {
            return View();
        }


        //quen mk
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy người dùng hoặc email chưa được xác nhận.");
                    return View(model);
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(
                    nameof(ResetPassword), "Account",
                    new { code, email = user.Email },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Đặt lại mật khẩu",
                    $"Tên đăng nhập: {user.UserName}\n" +
                    $"Click vào link sau để đặt lại mật khẩu: {callbackUrl}");


                ViewBag.SuccessMessage = "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư của bạn.";
                ModelState.Clear(); // Xoá model để input không giữ lại
            }

            return View(model);
        }



        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ResetPassword(string code = null, string email = null)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Phải cung cấp mã và email để đặt lại mật khẩu.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            var model = new ResetPasswordViewModel
            {
                Code = code,
                Email = email,
            };

            ViewBag.username = user.UserName;
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Không tiết lộ thông tin người dùng
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("EditProfile");
            }
            else
            {
                TempData["Error"] = "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại thông tin.";
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

    }

}
