using LHBooksWeb.Data;
using LHBooksWeb.Models.ViewModels;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LHBooksWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Các dữ liệu khác cho trang chủ

            // Truyền FlashSale Partial thông qua ViewComponent hoặc PartialView
            var flashSaleProducts = GetFlashSaleProducts();
            ViewBag.FlashSaleProducts = flashSaleProducts;

            return View();
        }

        [HttpGet]
        public IActionResult GetFlashSalePartial()
        {
            var flashSaleProducts = GetFlashSaleProducts();
            return PartialView("_FlashSalePartial", flashSaleProducts);
        }

        private List<FlashSaleProductViewModel> GetFlashSaleProducts()
        {
            var now = DateTime.Now;
            var activeFlashSale = _context.FlashSales
                .Where(f => f.IsActive && f.StartTime <= now && f.EndTime >= now)
                .OrderByDescending(f => f.StartTime)
                .FirstOrDefault();

            if (activeFlashSale == null)
                return new List<FlashSaleProductViewModel>();

            var flashSaleProducts = _context.FlashSaleProducts
                .Where(fp => fp.FlashSaleId == activeFlashSale.Id && fp.Product.IsActive)
                .Include(fp => fp.Product)
                    .ThenInclude(p => p.OrderDetails)
                .Include(fp => fp.Product)
                    .ThenInclude(p => p.ProductImage)
                .Select(fp => new FlashSaleProductViewModel
                {
                    Product = fp.Product,
                    FlashPrice = fp.FlashPrice,
                    Sold = fp.Sold,
                    QuantityLimit = fp.QuantityLimit,
                    FlashSaleId = fp.FlashSaleId
                    
                })
                .Take(10)
                .ToList();

            ViewBag.FlashSaleEndTime = activeFlashSale.EndTime;
            return flashSaleProducts;
        }

        public IActionResult Contact()
        {
            var contact = _context.Contact.FirstOrDefault();
            return View(contact);
        }

	}

}