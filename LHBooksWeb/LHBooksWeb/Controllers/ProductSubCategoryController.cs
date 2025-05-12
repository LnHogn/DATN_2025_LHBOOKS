using LHBooksWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Controllers
{
    public class ProductSubCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductSubCategoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string alias, int id, string sort, int publisherId = 0)
        {
            var cate = await _context.ProductSubCategories
                .Include(c => c.ProductCategory)
                .FirstOrDefaultAsync(c => c.Alias == alias || c.Id == id);

            ViewBag.CateName = cate.ProductCategory?.Title;
            ViewBag.CateId = cate.ProductCategory?.Id;
            ViewBag.CateAlias = cate.ProductCategory?.Alias;
            ViewBag.SubCateName = cate.Title;
            ViewBag.SubCateId = id;
            ViewBag.SubCateAlias = cate.Alias;

            var itemsQuery = _context.Products
        .Where(p => p.ProductSubCategoryId == id)
        .Include(p => p.ProductImage)
        .Include(p => p.Publisher)
        .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
        .AsQueryable();

            if (publisherId > 0)
            {
                itemsQuery = itemsQuery.Where(p => p.PublisherId == publisherId);
            }
            itemsQuery = sort switch
            {
                "price" => itemsQuery.OrderBy(p => p.Price),
                "name" => itemsQuery.OrderBy(p => p.Name),
                "mostViewed" => itemsQuery.OrderByDescending(p => p.ViewCount),
                _ => itemsQuery.OrderBy(p => p.Id)
            };
            var items = await itemsQuery.ToListAsync();

            var publishers = await _context.Publishers
        .OrderBy(p => p.Name)
        .ToListAsync(); // Truyền trực tiếp đối tượng Publisher đầy đủ

            ViewBag.Publishers = publishers;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductListPartial", items);
            }

            return View(items);
        }

    }
}
