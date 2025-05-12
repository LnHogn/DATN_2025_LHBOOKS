using LHBooksWeb.Data;
using LHBooksWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LHBooksWeb.Controllers
{
    // Controllers/CartController.cs
    public class CartController :Controller
    {
        private readonly CartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(CartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
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

    }
}
