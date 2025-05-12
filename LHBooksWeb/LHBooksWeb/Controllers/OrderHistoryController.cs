using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Controllers
{
    public class OrderHistoryController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderHistoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }


        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.FlashSale)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpGet]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.FlashSale)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            if (order.Status == OrderStatus.Pending)
            {
                foreach (var item in order.OrderDetails)
                {
                    // Nếu đơn hàng có FlashSaleId
                    if (item.FlashSaleId.HasValue)
                    {
                        // Tìm FlashSaleProduct tương ứng
                        var flashSaleProduct = _context.FlashSaleProducts
                            .FirstOrDefault(fsp =>
                                fsp.FlashSaleId == item.FlashSaleId &&
                                fsp.ProductId == item.ProductId);

                        if (flashSaleProduct != null)
                        {
                            flashSaleProduct.Sold -= item.Quantity;

                            // Bảo vệ nếu số lượng bán ra âm
                            if (flashSaleProduct.Sold < 0)
                                flashSaleProduct.Sold = 0;
                        }
                    }

                    // Tăng lại số lượng tồn kho sản phẩm
                    item.Product.Quantity += item.Quantity;
                }

                order.Status = OrderStatus.Cancelled;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Đơn hàng đã được hủy và số lượng đã được cập nhật.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng khi trạng thái không phải 'Chờ xác nhận'.";
            }

            return RedirectToAction("Details", new { id = orderId });
        }


    }
}
