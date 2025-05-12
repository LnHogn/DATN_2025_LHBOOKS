using LHBooksWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.ViewComponents
{
    public class SaleBooksViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SaleBooksViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var books = _context.Products
                .Where(p => p.IsSale && p.IsActive)
                .Include(p => p.ProductImage).Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)

                .OrderByDescending(p => p.PublishedDate)
                .Take(6)
                .ToList();

            return View(books);
        }
    }

}
