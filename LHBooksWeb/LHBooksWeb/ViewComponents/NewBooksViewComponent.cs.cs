using LHBooksWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.ViewComponents
{
    public class NewBooksViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NewBooksViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var books = _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.ProductImage).Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)

                .OrderByDescending(p => p.PublishedDate)
                .Take(10)
                .ToList();

            return View(books);
        }
    }

}
