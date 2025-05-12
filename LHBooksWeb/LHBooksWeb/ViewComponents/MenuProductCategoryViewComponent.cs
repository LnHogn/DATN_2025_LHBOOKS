using LHBooksWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MenuProductCategoryViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public MenuProductCategoryViewComponent(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _db.ProductCategories.ToListAsync();
        return View("_MenuProductCategory", items);
    }
}
