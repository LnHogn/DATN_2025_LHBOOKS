using LHBooksWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MenuLeftViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public MenuLeftViewComponent(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync(int? id)
    {
        if (id.HasValue)
        {
            ViewBag.CateId = id;

        }

        // Lấy danh mục chính cùng với các danh mục phụ liên quan
        var items = await _db.ProductCategories
                              .Include(c => c.ProductSubCategories) // Bao gồm danh mục phụ
                              .ToListAsync();

        return View("MenuLeft", items);
    }

}
