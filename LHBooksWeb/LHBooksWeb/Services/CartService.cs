using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LHBooksWeb.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public CartService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task AddToCartAsync(int productId, string productName, string productImage, decimal price, int quantity)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng");
            }

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    ProductImage = productImage,
                    Price = price,
                    Quantity = quantity,
                    UserId = userId
                };
                _context.CartItems.Add(cartItem);
            }
            await _context.SaveChangesAsync();
        }

 

        // Lấy tất cả sản phẩm trong giỏ hàng
        public async Task<List<CartItem>> GetCartItemsAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return new List<CartItem>();
            }
            return await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        }

        // Cập nhật số lượng sản phẩm
        public async Task UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                if (cartItem.Quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                await _context.SaveChangesAsync();
            }
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        // Tính tổng tiền giỏ hàng
        public async Task<decimal> GetCartTotalAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return 0;
            }

            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity * c.Price); // Hoặc c.SubTotal
        }



        // Xóa tất cả giỏ hàng sau khi đặt hàng thành công
        public async Task ClearCartAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }
    }
}
