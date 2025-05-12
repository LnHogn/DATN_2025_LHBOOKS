using LHBooksWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.ViewComponents
{
    public class FeaturedBooksViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public FeaturedBooksViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var books = _context.Products
                .Where(p => p.IsFeature && p.IsActive)
                .Include(p => p.ProductImage)
                .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)

                .OrderByDescending(p => p.PublishedDate)
                .Take(6)
                .ToList();

            return View(books);
        }
    }

}
