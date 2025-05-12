using LHBooksWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.ViewComponents
{
    public class HotBooksViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public HotBooksViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var hotBooks = await _context.Products
                .Where(p => p.IsHot && p.IsActive)
                .Include(p => p.ProductImage).Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)

                .OrderByDescending(p => p.ViewCount)
                .Take(8)
                .ToListAsync();

            return View(hotBooks);
        }
    }

}
