﻿using Microsoft.AspNetCore.Authorization;
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
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;


namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Employee")]

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

        [Authorize(Roles = "Admin")]
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



        [Authorize(Roles = "Admin")]

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
                    TempData["ErrorMessage"] = "Quyền được chọn không hợp lệ.";
                    await LoadRolesAsync(model.Role); // Load lại roles và giữ lại lựa chọn cũ (nếu có thể)
                    return View(model);
                }

                // Kiểm tra Username đã tồn tại
                var existingUserByUsername = await _userManager.FindByNameAsync(model.UserName);
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                    TempData["ErrorMessage"] = "Tên đăng nhập đã tồn tại.";
                    await LoadRolesAsync(model.Role);
                    return View(model);
                }

                // Kiểm tra Email đã tồn tại
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    TempData["ErrorMessage"] = "Email đã được sử dụng.";
                    await LoadRolesAsync(model.Role);
                    return View(model);
                }

                // Kiểm tra SĐT đã tồn tại
                var existingUserByPhone = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.Phone);
                if (existingUserByPhone != null)
                {
                    ModelState.AddModelError("Phone", "Số điện thoại đã được sử dụng.");
                    TempData["ErrorMessage"] = "Số điện thoại đã được sử dụng.";
                    await LoadRolesAsync(model.Role);
                    return View(model);
                }


                // Tạo đối tượng ApplicationUser từ ViewModel
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    isActive = true,
                    PhoneNumber = model.Phone,
                    FullName = model.FullName, 
                    EmailConfirmed = true 
                };

                // Thử tạo user mới với mật khẩu đã cung cấp
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {

                    // Thêm user vào Role đã chọn
                    var addToRoleResult = await _userManager.AddToRoleAsync(user, model.Role);

                    if (addToRoleResult.Succeeded)
                    {
                        // Thành công, chuyển hướng đến trang danh sách tài khoản (hoặc trang khác)
                        TempData["SuccessMessage"] = $"Tạo tài khoản {user.UserName} thành công!";
                        return RedirectToAction("Index", "Account"); // Hoặc tên controller quản lý user của bạn
                    }
                    else
                    {
                        // Xử lý lỗi khi thêm Role (hiếm khi xảy ra nếu role đã được kiểm tra tồn tại)
                        foreach (var error in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        TempData["ErrorMessage"] = $"Đã tạo người dùng nhưng không thể gán quyền {model.Role}.";
                        // Lưu ý: User đã được tạo, nhưng chưa có Role.
                        // Cân nhắc xóa user vừa tạo nếu việc gán role là bắt buộc và thất bại.
                    }
                }
                else
                {
                    // Nếu tạo user thất bại, thêm lỗi vào ModelState để hiển thị trên View
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["ErrorMessage"] = "Không thể tạo tài khoản. Vui lòng kiểm tra thông tin.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";
            }

            await LoadRolesAsync(model.Role); 
            return View(model);
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
                TempData["ErrorMessage"] = "Thông tin đăng nhập không hợp lệ.";
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Xin chào, {model.UserName}! Đăng nhập thành công.";
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                TempData["ErrorMessage"] = "Tài khoản đã bị khóa. Vui lòng thử lại sau.";
                return View("Lockout");
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản chưa được xác nhận.");
                TempData["ToastrWarning"] = "Tài khoản chưa được xác nhận.";
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Đăng nhập không thành công. Vui lòng kiểm tra lại tài khoản và mật khẩu.");
            TempData["ErrorMessage"] = "Đăng nhập không thành công. Vui lòng kiểm tra lại tài khoản và mật khẩu.";
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
            return RedirectToAction("LoginAdmin");
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
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateUserStatus(string userId)
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

            // Cập nhật trạng thái IsActive của người dùng thành false
            user.isActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Cập nhật trạng thái người dùng {user.UserName} thành công.";
                return Json(new { success = true, message = "Cập nhật trạng thái người dùng thành công.", toastr = "success" });
            }

            return Json(new { success = false, message = "Cập nhật trạng thái người dùng thất bại.", errors = result.Errors, toastr = "error" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> EnableUserStatus(string userId)
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

            // Cập nhật trạng thái IsActive của người dùng thành false
            user.isActive = true;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Cập nhật trạng thái người dùng {user.UserName} thành công.";
                return Json(new { success = true, message = "Cập nhật trạng thái người dùng thành công.", toastr = "success" });
            }

            return Json(new { success = false, message = "Cập nhật trạng thái người dùng thất bại.", errors = result.Errors, toastr = "error" });
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var viewModel = new UserDetailViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Không có",
                isActive = user.isActive ? "Hoạt động" : "Không hoạt động"
            };

            return View(viewModel);
        }

    }
}