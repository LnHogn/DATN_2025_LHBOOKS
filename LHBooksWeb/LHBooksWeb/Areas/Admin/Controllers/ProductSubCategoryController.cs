using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class ProductSubCategoryController : BaseController
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductSubCategoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var subCategories = await db.ProductSubCategories
                .Include(p => p.ProductCategory)
                .ToListAsync();
            return View(subCategories);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục phụ.";
                return NotFound();
            }

            var subCategory = await db.ProductSubCategories
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subCategory == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục phụ yêu cầu.";
                return NotFound();
            }

            return View(subCategory);
        }

        public IActionResult Create()
        {
            ViewData["ProductCategoryId"] = new SelectList(db.ProductCategories, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductSubCategory subCategory)
        {
            ModelState.Remove("ProductCategory");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                subCategory.CreatedBy = user?.UserName;
                subCategory.CreatedDate = DateTime.Now;
                subCategory.ModifiedDate = DateTime.Now;
                subCategory.Alias = LHBooksWeb.Common.Filter.FilterChar(subCategory.Title);
                db.Add(subCategory);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã tạo danh mục phụ '{subCategory.Title}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductCategoryId"] = new SelectList(db.ProductCategories, "Id", "Title", subCategory.ProductCategoryId);
            TempData["WarningMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return View(subCategory);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục phụ.";
                return NotFound();
            }

            var subCategory = await db.ProductSubCategories.FindAsync(id);
            if (subCategory == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục phụ yêu cầu.";
                return NotFound();
            }

            ViewData["ProductCategoryId"] = new SelectList(db.ProductCategories, "Id", "Title", subCategory.ProductCategoryId);
            return View(subCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductSubCategory subCategory)
        {
            if (id != subCategory.Id)
            {
                TempData["ErrorMessage"] = "ID danh mục không khớp.";
                return NotFound();
            }
            ModelState.Remove("ProductCategory");

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    subCategory.ModifiedBy = user?.UserName;
                    subCategory.Alias = Common.Filter.FilterChar(subCategory.Title);
                    db.Update(subCategory);
                    await db.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật danh mục phụ '{subCategory.Title}' thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductSubCategoryExists(subCategory.Id))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy danh mục phụ cần cập nhật.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi cập nhật đồng thời. Vui lòng thử lại.";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductCategoryId"] = new SelectList(db.ProductCategories, "Id", "Title", subCategory.ProductCategoryId);
            TempData["WarningMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return View(subCategory);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var subcate = db.ProductSubCategories.Find(id);
            if (subcate == null)
            {
                return Json(new { success = false, message = "Danh mục phụ không tồn tại!" });
            }

            // Kiểm tra xem có sản phẩm nào thuộc danh mục phụ này không
            bool hasProducts = db.Products.Any(p => p.ProductSubCategoryId == id);
            if (hasProducts)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể xóa danh mục phụ vì đang có sản phẩm thuộc danh mục phụ này!"
                });
            }

            try
            {
                string subCategoryName = subcate.Title;
                db.ProductSubCategories.Remove(subcate);
                db.SaveChanges();
                return Json(new { success = true, message = $"Xóa danh mục phụ '{subCategoryName}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa thất bại! " + ex.Message });
            }
        }


        private bool ProductSubCategoryExists(int id)
        {
            return db.ProductSubCategories.Any(e => e.Id == id);
        }
    }
}
