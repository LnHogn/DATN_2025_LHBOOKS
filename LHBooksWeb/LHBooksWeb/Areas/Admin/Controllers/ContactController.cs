using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LHBooksWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin,Manager")]
	public class ContactController : BaseController
	{
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ContactController(ApplicationDbContext context,IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
		}
		public async Task<IActionResult> Index()
		{
			var contact = await _context.Contact.ToListAsync();
			return View(contact);
		}


        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _context.Contact.FirstOrDefaultAsync();
            if (contact == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin";
                return NotFound();
            }
            return View(contact);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            var exited_contact = _context.Contact.FirstOrDefault(); // chỉ lấy 1 contact, giả định chỉ có 1

            if (ModelState.IsValid)
            {
                // Nếu người dùng có upload ảnh mới
                if (contact.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads/logo");
                    Directory.CreateDirectory(uploadsDir); // đảm bảo thư mục tồn tại

                    // Xoá ảnh cũ nếu có (và không phải ảnh mặc định nếu bạn dùng mặc định)
                    if (!string.IsNullOrEmpty(exited_contact.LogoImg))
                    {
                        string oldImagePath = Path.Combine(uploadsDir, exited_contact.LogoImg);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu ảnh mới
                    string imageName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(contact.ImageUpload.FileName);
                    string filePath = Path.Combine(uploadsDir, imageName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await contact.ImageUpload.CopyToAsync(fs);
                    }

                    exited_contact.LogoImg = imageName;
                }

                // Cập nhật các thông tin còn lại
                exited_contact.Name = contact.Name;
                exited_contact.Map = contact.Map;
                exited_contact.Email = contact.Email;
                exited_contact.Phone = contact.Phone;
                exited_contact.Description = contact.Description;

                _context.Update(exited_contact);
                await _context.SaveChangesAsync();

                TempData["success"] = "Cập nhật thành công";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Có lỗi khi cập nhật";
            return View(contact);
        }

    }
}
