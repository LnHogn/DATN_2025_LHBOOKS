using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : BaseController
    {
        private readonly ApplicationDbContext db;

        // Sử dụng Dependency Injection thông qua constructor
        public OrderController(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }

        public IActionResult Index()
        {
            var orders = db.Orders
                           .OrderByDescending(o => o.OrderDate)
                           .ToList();

            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await db.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product) // Adding Product include if needed
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.FlashSale)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }




        [HttpPost]
        public async Task<JsonResult> UpdateTT(int id, int trangthai)
        {
            try
            {
                var order = await db.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.FlashSale)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return Json(new { Success = false, Message = "Đơn hàng không tồn tại!" });
                }

                // Nếu trạng thái mới là Cancelled và trạng thái cũ không phải Cancelled
                if ((OrderStatus)trangthai == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
                {
                    foreach (var item in order.OrderDetails)
                    {
                        // Cập nhật lại số lượng trong kho
                        item.Product.Quantity += item.Quantity;

                        // Nếu có FlashSale, cập nhật lại số lượng đã bán
                        if (item.FlashSaleId.HasValue)
                        {
                            var flashSaleProduct = await db.FlashSaleProducts
                                .FirstOrDefaultAsync(fsp =>
                                    fsp.FlashSaleId == item.FlashSaleId &&
                                    fsp.ProductId == item.ProductId);

                            if (flashSaleProduct != null)
                            {
                                flashSaleProduct.Sold -= item.Quantity;
                                if (flashSaleProduct.Sold < 0)
                                    flashSaleProduct.Sold = 0;
                            }
                        }
                    }
                }

                order.Status = (OrderStatus)trangthai;
                order.ModifiedDate = DateTime.Now;

                await db.SaveChangesAsync();

                return Json(new { success = true, statusText = GetEnumDisplayName(order.Status) });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }


        public static string GetEnumDisplayName(Enum value)
        {
            var displayAttr = value.GetType()
                .GetField(value.ToString())
                ?.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            return displayAttr?.Name ?? value.ToString();
        }

        public IActionResult PrintInvoice(int orderId)
        {
            var order = db.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product) // Lấy thông tin sản phẩm
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.FlashSale) // Lấy thông tin FlashSale
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();
            var totalWeight = order.OrderDetails.Sum(od => od.Quantity * od.Product.Weight);
            ViewBag.TotalWeight = totalWeight;
            return View(order);
        }


    }
}
