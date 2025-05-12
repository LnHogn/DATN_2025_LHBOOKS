using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using LHBooksWeb.Data;
using LHBooksWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        //public async Task<IActionResult> Index()
        //{
        //    var users = await _dbContext.Users.ToListAsync();
        //    return View(users);
        //}
        public async Task<IActionResult> Index()
        {
            var users = await _dbContext.Users.ToListAsync();

            var rolesForUsers = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var roles = await _dbContext.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Join(_dbContext.Roles,
                                      ur => ur.RoleId,
                                      r => r.Id,
                                      (ur, r) => r.Name)
                                .ToListAsync();

                rolesForUsers[user.UserName] = roles;
            }

            ViewBag.RolesForUsers = rolesForUsers;

            return View(users);
        }




        public async Task<IActionResult> Create()
        {
            await LoadRolesAsync();
            //ViewData["ActiveController"] = ControllerContext.RouteData.Values["controller"]?.ToString();
            return View();
        }

        // POST: Admin/Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Role được chọn có tồn tại không
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                {
                    ModelState.AddModelError("Role", "Quyền được chọn không hợp lệ.");
                    TempData["ToastrError"] = "Quyền được chọn không hợp lệ.";
                    await LoadRolesAsync(model.Role); // Load lại roles và giữ lại lựa chọn cũ (nếu có thể)
                    return View(model);
                }

                // Tạo đối tượng ApplicationUser từ ViewModel
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    FullName = model.FullName, // Gán FullName nếu có trong ApplicationUser
                    EmailConfirmed = true // Hoặc false nếu bạn muốn quy trình xác thực email
                    // Thêm các thuộc tính khác nếu cần
                };

                // Thử tạo user mới với mật khẩu đã cung cấp
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.UserName} created successfully."); // Ghi log thành công

                    // Thêm user vào Role đã chọn
                    var addToRoleResult = await _userManager.AddToRoleAsync(user, model.Role);

                    if (addToRoleResult.Succeeded)
                    {
                        _logger.LogInformation($"User {user.UserName} added to role {model.Role}."); // Ghi log gán role thành công
                        // Thành công, chuyển hướng đến trang danh sách tài khoản (hoặc trang khác)
                        TempData["ToastrSuccess"] = $"Tạo tài khoản {user.UserName} thành công!";
                        return RedirectToAction("Index", "Account"); // Hoặc tên controller quản lý user của bạn
                    }
                    else
                    {
                        // Ghi log lỗi khi gán role
                        _logger.LogError($"Error adding user {user.UserName} to role {model.Role}.");
                        // Xử lý lỗi khi thêm Role (hiếm khi xảy ra nếu role đã được kiểm tra tồn tại)
                        foreach (var error in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                            _logger.LogError($"Error detail: {error.Code} - {error.Description}");
                        }
                        TempData["ToastrError"] = $"Đã tạo người dùng nhưng không thể gán quyền {model.Role}.";
                        // Lưu ý: User đã được tạo, nhưng chưa có Role.
                        // Cân nhắc xóa user vừa tạo nếu việc gán role là bắt buộc và thất bại.
                        // await _userManager.DeleteAsync(user); // Cân nhắc kỹ lưỡng khi dùng lệnh này
                    }
                }
                else
                {
                    // Ghi log lỗi khi tạo user
                    _logger.LogError($"Error creating user {model.UserName}.");
                    // Nếu tạo user thất bại, thêm lỗi vào ModelState để hiển thị trên View
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        _logger.LogError($"Error detail: {error.Code} - {error.Description}");
                    }
                    TempData["ToastrError"] = "Không thể tạo tài khoản. Vui lòng kiểm tra thông tin.";
                }
            }
            else
            {
                TempData["ToastrError"] = "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";
            }

            // Nếu có lỗi (ModelState không valid hoặc tạo user/gán role thất bại), load lại roles và hiển thị lại form
            await LoadRolesAsync(model.Role); // Load lại danh sách Roles cho dropdown
            return View(model); // Trả về view với dữ liệu đã nhập và thông báo lỗi
        }

        [AllowAnonymous]
        public IActionResult LoginAdmin(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAdmin(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastrError"] = "Thông tin đăng nhập không hợp lệ.";
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Người dùng {UserName} đăng nhập thành công.", model.UserName);
                TempData["ToastrSuccess"] = $"Xin chào, {model.UserName}! Đăng nhập thành công.";
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Tài khoản {UserName} bị khóa.", model.UserName);
                TempData["ToastrError"] = "Tài khoản đã bị khóa. Vui lòng thử lại sau.";
                return View("Lockout");
            }

            if (result.IsNotAllowed)
            {
                _logger.LogWarning("Tài khoản {UserName} chưa được xác nhận.", model.UserName);
                ModelState.AddModelError(string.Empty, "Tài khoản chưa được xác nhận.");
                TempData["ToastrWarning"] = "Tài khoản chưa được xác nhận.";
                return View(model);
            }

            _logger.LogWarning("Đăng nhập thất bại cho người dùng {UserName}.", model.UserName);
            ModelState.AddModelError(string.Empty, "Đăng nhập không thành công. Vui lòng kiểm tra lại tài khoản và mật khẩu.");
            TempData["ToastrError"] = "Đăng nhập không thành công. Vui lòng kiểm tra lại tài khoản và mật khẩu.";
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            TempData["ToastrInfo"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Dashboard");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task LoadRolesAsync(string selectedRole = null)
        {
            // Lấy tất cả các roles từ database, sắp xếp theo tên
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            // Tạo SelectList, giá trị là Name, text hiển thị cũng là Name
            ViewData["Role"] = new SelectList(roles, "Name", "Name", selectedRole);
        }

        // POST: Admin/Account/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "ID người dùng không hợp lệ.", toastr = "error" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại.", toastr = "error" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.UserName} deleted successfully.");
                TempData["ToastrSuccess"] = $"Xóa người dùng {user.UserName} thành công.";
                return Json(new { success = true, message = "Xóa người dùng thành công.", toastr = "success" });
            }

            _logger.LogError($"Error deleting user {user.UserName}.");
            return Json(new { success = false, message = "Xóa người dùng thất bại.", errors = result.Errors, toastr = "error" });
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }



    }
}