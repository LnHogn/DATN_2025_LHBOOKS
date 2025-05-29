using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class PublisherController : BaseController
    {
        private readonly ApplicationDbContext db;
        public PublisherController(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var pub = await db.Publishers
                .Include(a => a.Products) // Include để nạp danh sách tác phẩm
                .ToListAsync();

            return View(pub);
        }



        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên
                bool isExist = await db.Publishers.AnyAsync(p => p.Name == publisher.Name);
                if (isExist)
                {
                    TempData["ErrorMessage"] = "Tên nhà xuất bản đã tồn tại.";
                    return View(publisher);
                }

                db.Publishers.Add(publisher);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"NXB '{publisher.Name}' đã được thêm thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(publisher);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var pub = db.Publishers.FirstOrDefault(a => a.Id == id);
            if (pub == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nxb";
                return NotFound();
            }
            return View(pub);
        }

        // Edit Method (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publisher pub)
        {
            if (id != pub.Id)
            {
                TempData["ErrorMessage"] = "ID NXB không hợp lệ";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Update(pub);
                    await db.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"NXB '{pub.Name}' đã được cập nhật thành công";
                }
                catch (Exception)
                {
                    if (!db.Publishers.Any(a => a.Id == pub.Id))
                    {
                        TempData["ErrorMessage"] = "NXB không tồn tại";
                        return NotFound();
                    }
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật NXB";
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pub);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var pub = await db.Publishers.FindAsync(id);
            if (pub == null)
            {
                return Json(new { success = false, message = "NXB không tồn tại!" });
            }

            // Kiểm tra xem có sản phẩm nào thuộc NXB này không
            bool hasProducts = db.Products.Any(p => p.PublisherId == id);
            if (hasProducts)
            {
                return Json(new { success = false, message = "Không thể xóa NXB vì đang có sản phẩm thuộc NXB này!" });
            }

            try
            {
                db.Publishers.Remove(pub);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "NXB đã được xóa thành công";
                return Json(new { success = true, message = "NXB đã được xóa thành công" });
            }
            catch
            {
                return Json(new { success = false, message = "Xóa thất bại!" });
            }
        }

        [HttpGet]
        public JsonResult SearchPub(string term)
        {
            var pub = db.Publishers
                .Where(a => a.Name.Contains(term))
                .Select(a => new {
                    label = a.Name,
                    id = a.Id
                })
                .ToList();

            return Json(pub); // Nếu dùng ASP.NET MVC
        }

    }
}

