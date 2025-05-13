using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class ProductCategoryController : BaseController
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductCategoryController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            db = dbContext;
            _userManager = userManager;
        }

        // GET: ProductCategory/Index
        public async Task<IActionResult> Index()
        {
            return View(await db.ProductCategories.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategory model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    model.CreatedBy = user?.UserName;
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDate = DateTime.Now;
                    model.Alias = LHBooksWeb.Common.Filter.FilterChar(model.Title);
                    db.ProductCategories.Add(model);
                    await db.SaveChangesAsync();

                    // Thêm thông báo thành công
                    TempData["SuccessMessage"] = $"Đã tạo danh mục '{model.Title}' thành công!";

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Log lỗi để debug
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
                    }

                    // Thông báo lỗi validation
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.";
                }
            }
            catch (Exception ex)
            {
                // Log exception
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu: " + ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                // Thông báo lỗi
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu dữ liệu: " + ex.Message;
            }

            return View(model);
        }

        // GET: ProductCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID danh mục.";
                return NotFound();
            }

            var productCategory = await db.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục yêu cầu.";
                return NotFound();
            }
            return View(productCategory);
        }

        // POST: ProductCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCategory productCategory)
        {
            if (id != productCategory.Id)
            {
                TempData["ErrorMessage"] = "ID danh mục không khớp.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    productCategory.ModifiedBy = user?.UserName;
                    productCategory.ModifiedDate = DateTime.Now;
                    productCategory.Alias = LHBooksWeb.Common.Filter.FilterChar(productCategory.Title);

                    db.Update(productCategory);
                    await db.SaveChangesAsync();

                    // Thông báo thành công
                    TempData["SuccessMessage"] = $"Cập nhật danh mục '{productCategory.Title}' thành công!";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductCategoryExists(productCategory.Id))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy danh mục cần cập nhật.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi cập nhật đồng thời. Vui lòng thử lại.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật: " + ex.Message;
                }
            }
            else
            {
                TempData["WarningMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.";
            }

            return View(productCategory);
        }

        [HttpPost]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var cate = db.ProductCategories.Find(id);
            if (cate == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Danh mục không tồn tại!"
                });
            }

            // Kiểm tra có danh mục phụ không
            bool hasSubCategories = db.ProductSubCategories.Any(sc => sc.ProductCategoryId == id);

            // Kiểm tra có sản phẩm trực tiếp thuộc danh mục chính không
            bool hasProducts = db.Products.Any(p => p.ProductCategoryId == id);

            if (hasSubCategories || hasProducts)
            {
                string reason = "";
                if (hasSubCategories && hasProducts)
                {
                    reason = "danh mục phụ và sản phẩm";
                }
                else if (hasSubCategories)
                {
                    reason = "danh mục phụ";
                }
                else if (hasProducts)
                {
                    reason = "sản phẩm";
                }

                return Json(new
                {
                    success = false,
                    message = $"Không thể xóa danh mục vì đang có {reason} thuộc danh mục này!"
                });
            }

            try
            {
                string categoryName = cate.Title;
                db.ProductCategories.Remove(cate);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Đã xóa danh mục '{categoryName}' thành công!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Xóa thất bại! " + ex.Message
                });
            }
        }



        private bool ProductCategoryExists(int id)
        {
            return db.ProductCategories.Any(e => e.Id == id);
        }
    }
}
