using LHBooksWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.ViewComponents
{
    public class BestSellerBooksViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public BestSellerBooksViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var books = _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.ProductImage)
                .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                .Include(p => p.OrderDetails)

                .OrderByDescending(p => p.OrderDetails.Sum(od => od.Quantity))
                .Take(10)
                .ToList();

            return View(books);
        }

    }
}
