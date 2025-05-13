using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee,Manager")]
    public class AuthorController : BaseController
    {
        private readonly ApplicationDbContext db;
        public AuthorController(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var authors = await db.Authors
                .ToListAsync();

            return View(authors);
        }



        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                db.Authors.Add(author);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tác giả '{author.Name}' đã được thêm thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var author = await db.Authors.FirstOrDefaultAsync(a => a.Id == id);
            if (author == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tác giả";
                return NotFound();
            }
            return View(author);
        }

        // Edit Method (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.Id)
            {
                TempData["ErrorMessage"] = "ID tác giả không hợp lệ";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Update(author);
                    await db.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Tác giả '{author.Name}' đã được cập nhật thành công";
                }
                catch (Exception)
                {
                    if (!db.Authors.Any(a => a.Id == author.Id))
                    {
                        TempData["ErrorMessage"] = "Tác giả không tồn tại";
                        return NotFound();
                    }
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật tác giả";
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var author = db.Authors.Find(id);
            if (author == null)
            {
                return Json(new { success = false, message = "Tác giả không tồn tại!" });
            }

            try
            {
                db.Authors.Remove(author);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tác giả đã được xóa thành công";
                return Json(new { success = true, message = "Tác giả đã được xóa thành công" });
            }
            catch
            {
                return Json(new { success = false, message = "Xóa thất bại!" });
            }
        }

        [HttpGet]
        public JsonResult SearchAuthors(string term)
        {
            var authors = db.Authors
                .Where(a => a.Name.Contains(term)) // có dấu cũng match
                .Select(a => new {
                    label = a.Name,
                    value = a.Name,
                    id = a.Id
                })
                .ToList();

            return Json(authors);
        }



    }
}