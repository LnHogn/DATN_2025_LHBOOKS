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
        public async Task AddToCartAsync(int productId, string productName, string productImage, decimal price, int quantity, bool isSelected)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại.");
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            int totalQuantity = (cartItem?.Quantity ?? 0) + quantity;
            if (totalQuantity > product.Quantity)
            {
                throw new InvalidOperationException("Sản phẩm không đủ hàng trong kho.");
            }

            if (cartItem != null)
            {
                cartItem.Quantity = totalQuantity;
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
                    UserId = userId,
                    IsSelected = isSelected
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
            return await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<CartItem>> GetSelectedCartItemsAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return new List<CartItem>();
            }

            return await _context.CartItems
                .Where(c => c.UserId == userId && c.IsSelected == true)
                .ToListAsync();
        }



        // Cập nhật số lượng sản phẩm
        public async Task UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItem = await _context.CartItems
                .Include(c => c.Product) // để truy cập tồn kho sản phẩm
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    // Nếu số lượng yêu cầu <= 0 thì xóa khỏi giỏ hàng
                    _context.CartItems.Remove(cartItem);
                }
                else if (quantity > cartItem.Product.Quantity)
                {
                    // Nếu yêu cầu vượt quá tồn kho, không cập nhật và có thể báo lỗi
                    throw new InvalidOperationException("Sản phẩm không đủ hàng trong kho.");
                }
                else
                {
                    // Hợp lệ: cập nhật số lượng
                    cartItem.Quantity = quantity;
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

        public async Task RemoveSelectedItemsAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return;

            var selectedItems = await _context.CartItems
                .Where(c => c.UserId == userId && c.IsSelected)
                .ToListAsync();

            if (selectedItems.Any())
            {
                _context.CartItems.RemoveRange(selectedItems);
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
                .Where(c => c.UserId == userId && c.IsSelected)
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
