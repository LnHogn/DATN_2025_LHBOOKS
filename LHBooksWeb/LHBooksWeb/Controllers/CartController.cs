using LHBooksWeb.Data;
using LHBooksWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LHBooksWeb.Controllers
{
    // Controllers/CartController.cs
    public class CartController :Controller
    {
        private readonly CartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public CartController(CartService cartService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _cartService = cartService;
            _userManager = userManager;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            var total = await _cartService.GetCartTotalAsync();
            ViewBag.CartTotal = total;

            // Lấy thông tin người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            ViewBag.UserInfo = user;

            return View(cartItems);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, string productName, string productImage, decimal price, int quantity = 1)
        {
            await _cartService.AddToCartAsync(productId, productName, productImage, price, quantity);
            return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng" });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            await _cartService.UpdateQuantityAsync(cartItemId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(cartItemId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            int totalQuantity = cartItems.Sum(item => item.Quantity);
            return Json(totalQuantity);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSelection(int cartItemId, bool isSelected)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.IsSelected = isSelected;
                await _context.SaveChangesAsync();
                return Ok(); // Trả về mã thành công
            }

            return BadRequest("Không tìm thấy sản phẩm trong giỏ hàng.");
        }

        [HttpPost]
        public async Task<IActionResult> GetSelectedTotal()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var selectedItems = await _context.CartItems
                .Where(c => c.UserId == userId && c.IsSelected)
                .ToListAsync();

            var selectedTotal = selectedItems.Sum(i => i.Price * i.Quantity);

            return Json(new { total = selectedTotal.ToString("N0") + " đ" });
        }

    }
}
