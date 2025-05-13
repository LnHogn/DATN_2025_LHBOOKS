using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using LHBooksWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TextTemplating;

namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class ProductsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Product/Index
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductSubCategory)
                .Include(p => p.Publisher)
                .ToListAsync();

            return View(products);
        }


        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCategories = _context.ProductSubCategories
                .Where(sc => sc.ProductCategoryId == categoryId)
                .Select(sc => new {
                    sc.Id,
                    sc.Title
                }).ToList();

            return Json(subCategories);
        }

        [HttpGet]
        public JsonResult GetPublisher(int publisherId)
        {
            var publisher = _context.Publishers
                .Where(sc => sc.Id == publisherId)
                .ToList();

            return Json(publisherId);
        }


        // GET: Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductSubCategory)
                .Include(p => p.ProductImage)
                .Include(p => p.Publisher)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm yêu cầu.";
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ProductCategories"] = new SelectList(await _context.ProductCategories.ToListAsync(), "Id", "Title");
            //ViewData["ProductSubCategories"] = new SelectList(await _context.ProductSubCategories.ToListAsync(), "Id", "Title");

            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            try
            {
                ModelState.Remove("PublisherName");
                ModelState.Remove("Translator");

                if (model.Price == 0)
                {
                    ModelState.AddModelError("Price", "Vui lòng nhập giá gốc");
                }

                if (model.PriceSale == 0)
                {
                    ModelState.AddModelError("PriceSale", "Vui lòng nhập giá bán");
                }

                if (ModelState.IsValid)
                {
                    
                    // Tạo sản phẩm mới
                    var product = new Product
                    {
                        ProductCode = model.ProductCode,
                        Name = model.Name,
                        Description = model.Description,
                        Detail = model.Detail,
                        Price = model.Price,
                        PriceSale = model.PriceSale,
                        Quantity = model.Quantity,
                        ProductCategoryId = model.ProductCategoryId,
                        ProductSubCategoryId = model.ProductSubCategoryId,
                        AuthorName = model.AuthorName,
                        PublisherId = model.PublisherId,
                        Alias = LHBooksWeb.Common.Filter.FilterChar(model.Name),
                        IsActive = model.IsActive,
                        IsHot = model.IsHot,
                        IsSale = model.IsSale,
                        IsFeature = model.IsFeature,
                        ViewCount = 0,
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        PublishedDate = DateTime.Now,
                        PublishYear = model.PublishYear,
                        Language = model.Language,
                        Weight = model.Weight,
                        PageCount = model.PageCount,
                        Format = model.Format,
                        Translator = model.Translator,
                        Image = "/Uploads/product/default-image.jpg" // Ảnh mặc định
                    };

                    // QUAN TRỌNG: Lưu product vào database TRƯỚC để có Id
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    // Xử lý upload ảnh SAU KHI product đã có Id
                    if (model.ImageFiles != null && model.ImageFiles.Count > 0)
                    {
                        // Đảm bảo thư mục uploads tồn tại
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Tạo thư mục product trong uploads nếu chưa có
                        string productFolder = Path.Combine(uploadsFolder, "product");
                        if (!Directory.Exists(productFolder))
                        {
                            Directory.CreateDirectory(productFolder);
                        }

                        List<ProductImage> productImages = new List<ProductImage>();

                        for (int i = 0; i < model.ImageFiles.Count; i++)
                        {
                            var file = model.ImageFiles[i];
                            if (file != null && file.Length > 0)
                            {
                                try
                                {
                                    // Tạo tên file duy nhất
                                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                    string filePath = Path.Combine(productFolder, fileName);

                                    // Lưu file vào thư mục
                                    using (var stream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await file.CopyToAsync(stream);
                                    }

                                    // Tạo đường dẫn tương đối để lưu vào database
                                    string relativePath = "/Uploads/product/" + fileName;

                                    // Tạo bản ghi ProductImage
                                    var productImage = new ProductImage
                                    {
                                        ProductId = product.Id,
                                        Image = relativePath,
                                        IsDefault = i == model.DefaultImageId
                                    };

                                    // Thêm vào danh sách ảnh
                                    productImages.Add(productImage);

                                    // Nếu là ảnh mặc định, cập nhật luôn cho product
                                    if (productImage.IsDefault)
                                    {
                                        product.Image = relativePath;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log lỗi xử lý file
                                    ModelState.AddModelError("ImageFiles", $"Lỗi xử lý file {file.FileName}: {ex.Message}");
                                    TempData["ErrorMessage"] = $"Lỗi xử lý file {file.FileName}: {ex.Message}";
                                }
                            }
                        }

                        // Thêm tất cả ảnh vào database
                        if (productImages.Any())
                        {
                            _context.ProductImages.AddRange(productImages);

                            // Cập nhật sản phẩm nếu có thay đổi ảnh mặc định
                            _context.Products.Update(product);

                            await _context.SaveChangesAsync();
                        }
                    }

                    TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Hiển thị lỗi validation chi tiết
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            foreach (var error in state.Errors)
                            {
                                ModelState.AddModelError(string.Empty, $"Lỗi tại {key}: {error.ErrorMessage}");
                            }
                        }
                    }
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin nhập.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Xử lý lỗi database
                ModelState.AddModelError(string.Empty, $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}");
                TempData["ErrorMessage"] = "Lỗi cơ sở dữ liệu: " + (dbEx.InnerException?.Message ?? dbEx.Message);
                // _logger.LogError(dbEx, "Lỗi khi lưu sản phẩm vào cơ sở dữ liệu");
            }
            catch (IOException ioEx)
            {
                // Xử lý lỗi I/O (đọc/ghi file)
                ModelState.AddModelError(string.Empty, $"Lỗi đọc/ghi file: {ioEx.Message}");
                TempData["ErrorMessage"] = "Lỗi đọc/ghi file: " + ioEx.Message;
                // _logger.LogError(ioEx, "Lỗi khi xử lý file");
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
                // _logger.LogError(ex, "Lỗi không xác định khi tạo sản phẩm");
            }

            // Nếu có lỗi, chuẩn bị lại dữ liệu cho dropdowns và trả về view
            ViewData["ProductCategories"] = new SelectList(await _context.ProductCategories.ToListAsync(), "Id", "Title");
            ViewData["ProductSubCategories"] = new SelectList(await _context.ProductSubCategories.ToListAsync(), "Id", "Title");
            //ViewData["Authors"] = new SelectList(await _context.Authors.ToListAsync(), "Id", "Name");

            return View(model);
        }

        // GET: Product/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImage)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần chỉnh sửa.";
                return NotFound();
            }

            var publisher = _context.Publishers.FirstOrDefault(a => a.Id == product.PublisherId);
            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Description = product.Description,
                Detail = product.Detail,
                Price = product.Price,
                PriceSale = product.PriceSale,
                Quantity = product.Quantity,
                ProductCategoryId = product.ProductCategoryId,
                ProductSubCategoryId = product.ProductSubCategoryId,
                AuthorName = product.AuthorName,
                PublisherId = product.PublisherId,
                PublisherName = publisher?.Name,
                Alias = product.Alias,
                IsActive = product.IsActive,
                IsHot = product.IsHot,
                IsSale = product.IsSale,
                IsFeature = product.IsFeature,
                PublishYear = product.PublishYear,
                Language = product.Language,
                Weight  = product.Weight,
                PageCount = product.PageCount,
                Format = product.Format,
                Translator = product.Translator,
            };

            ViewData["ProductCategories"] = new SelectList(await _context.ProductCategories.ToListAsync(), "Id", "Title", product.ProductCategoryId);
            ViewData["ProductSubCategories"] = new SelectList(await _context.ProductSubCategories.ToListAsync(), "Id", "Title", product.ProductSubCategoryId);
            //ViewData["Authors"] = new SelectList(await _context.Authors.ToListAsync(), "Id", "Name", product.AuthorId);


            ViewBag.ProductImages = product.ProductImage.ToList();

            return View(viewModel);
        }

        //POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "ID sản phẩm không khớp với dữ liệu gửi lên.";
                return NotFound();
            }

            ModelState.Remove("PublisherName");
            ModelState.Remove("Translator");
            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần cập nhật.";
                        return NotFound();
                    }

                    // Cập nhật thông tin sản phẩm
                    product.ProductCode = model.ProductCode;
                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.Detail = model.Detail;
                    product.Price = model.Price;
                    product.PriceSale = model.PriceSale;
                    product.Quantity = model.Quantity;
                    product.ProductCategoryId = model.ProductCategoryId;
                    product.ProductSubCategoryId = model.ProductSubCategoryId;
                    product.AuthorName = model.AuthorName;
                    product.PublisherId = model.PublisherId;
                    product.Alias = LHBooksWeb.Common.Filter.FilterChar(model.Name);
                    product.IsActive = model.IsActive;
                    product.IsHot = model.IsHot;
                    product.IsSale = model.IsSale;
                    product.IsFeature = model.IsFeature;
                    product.ModifiedDate = DateTime.Now;
                    product.ModifiedBy = User.Identity.Name;
                    product.PublishYear = model.PublishYear;
                    product.Language = model.Language;
                    product.Weight = model.Weight;
                    product.PageCount = model.PageCount;
                    product.Format = model.Format;
                    product.Translator = model.Translator;

                    _context.Update(product);

                    if (Request.Form.ContainsKey("imageSelection"))
                    {
                        string selectedValue = Request.Form["imageSelection"];
                        if (selectedValue.StartsWith("existing-") && int.TryParse(selectedValue.Replace("existing-", ""), out int selectedImageId))
                        {
                            var existingImages = await _context.ProductImages.Where(p => p.ProductId == id).ToListAsync();
                            foreach (var img in existingImages)
                            {
                                img.IsDefault = img.Id == selectedImageId;
                                _context.Update(img);

                                if (img.IsDefault)
                                {
                                    product.Image = img.Image;
                                    _context.Update(product);
                                }
                            }
                        }
                        else if (selectedValue.StartsWith("new-"))
                        {
                            int newImageIndex = int.Parse(selectedValue.Replace("new-", ""));
                            var newImageFile = model.ImageFiles.ElementAtOrDefault(newImageIndex);

                            if (newImageFile != null && newImageFile.Length > 0)
                            {
                                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Uploads");
                                string productFolder = Path.Combine(uploadsFolder, "product");

                                if (!Directory.Exists(productFolder))
                                {
                                    Directory.CreateDirectory(productFolder);
                                }

                                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(newImageFile.FileName);
                                string filePath = Path.Combine(productFolder, fileName);

                                // Lưu file
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await newImageFile.CopyToAsync(stream);
                                }

                                // Tạo bản ghi ProductImage cho ảnh mới
                                var productImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    Image = "/Uploads/product/" + fileName,
                                    IsDefault = true
                                };

                                _context.ProductImages.Add(productImage);

                                // Cập nhật ảnh chính cho sản phẩm
                                product.Image = "/Uploads/product/" + fileName;
                                _context.Update(product);
                            }
                        }

                    }
                    // Lưu các thay đổi vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();                  
                    TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(model.Id))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong cơ sở dữ liệu.";
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
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật sản phẩm: " + ex.Message;
                }
            }
            else
            {
                TempData["WarningMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin nhập.";
            }

            ViewData["ProductCategories"] = new SelectList(_context.ProductCategories, "Id", "Title", model.ProductCategoryId);
            ViewData["ProductSubCategories"] = new SelectList(_context.ProductSubCategories, "Id", "Title", model.ProductSubCategoryId);
            //ViewData["Authors"] = new SelectList(_context.Authors, "Id", "Name", model.AuthorId);
            ViewBag.ProductImages = await _context.ProductImages.Where(p => p.ProductId == id).ToListAsync();

            return View(model);
        }



        // POST: Product/DeleteImage/5
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ảnh cần xóa.";
                return Json(new { success = false, message = "Không tìm thấy ảnh" });
            }

            try
            {
                // Kiểm tra xem đây có phải là ảnh mặc định không
                bool isDefault = productImage.IsDefault;

                // Xóa file từ server
                string webRootPath = _hostEnvironment.WebRootPath;
                string imagePath = webRootPath + productImage.Image.Replace("/", "\\");

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                // Xóa bản ghi từ database
                _context.ProductImages.Remove(productImage);

                // Nếu đây là ảnh mặc định, chọn ảnh mặc định mới
                if (isDefault)
                {
                    var product = await _context.Products.FindAsync(productImage.ProductId);
                    var newDefaultImage = await _context.ProductImages
                        .Where(p => p.ProductId == productImage.ProductId && p.Id != id)
                        .FirstOrDefaultAsync();

                    if (newDefaultImage != null)
                    {
                        newDefaultImage.IsDefault = true;
                        product.Image = newDefaultImage.Image;
                        _context.Update(product);
                        _context.Update(newDefaultImage);
                    }
                    else
                    {
                        // Không còn ảnh nào, đặt ảnh sản phẩm về mặc định
                        product.Image = "/Uploads/product/default-image.jpg";
                        _context.Update(product);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa ảnh thành công.";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa ảnh: " + ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
        }

       
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}