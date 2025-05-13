using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace LHBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class FlashSalesController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public FlashSalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /FlashSales
        public async Task<IActionResult> Index()
        {
            // Kiểm tra và cập nhật trạng thái các Flash Sale hết hạn
            var flashSales = await _context.FlashSales.Include(f => f.FlashSaleProducts)
                                                       .ThenInclude(fp => fp.Product)
                                                       .ToListAsync();

            foreach (var flashSale in flashSales)
            {
                // Kiểm tra nếu Flash Sale đã hết hạn và đang còn active
                if (flashSale.EndTime < DateTime.Now && flashSale.IsActive)
                {
                    flashSale.IsActive = false; // Đánh dấu Flash Sale là không hoạt động
                    _context.Update(flashSale); // Cập nhật lại Flash Sale trong cơ sở dữ liệu
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về View với dữ liệu đã được cập nhật
            return View(flashSales);
        }


        // GET: /FlashSales/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /FlashSales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlashSale flashSale)
        {
            if (ModelState.IsValid)
            {
                _context.Add(flashSale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(flashSale);
        }

        // GET: /FlashSales/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var flashSale = await _context.FlashSales
                .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fp => fp.Product)
                .FirstOrDefaultAsync(fs => fs.Id == id);

            if (flashSale == null)
                return NotFound();

            // Danh sách tất cả sản phẩm đang hoạt động
            ViewBag.Products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            // Danh sách tất cả danh mục
            ViewBag.Categories = await _context.ProductCategories
                .OrderBy(c => c.Title)
                .ToListAsync();

            // Danh sách tất cả danh mục phụ
            ViewBag.Subcategories = await _context.ProductSubCategories
                .OrderBy(sc => sc.Title)
                .ToListAsync();

            return View(flashSale);
        }


        [HttpPost]
        public async Task<IActionResult> AddProduct(int flashSaleId, int productId, int discountPercent, int quantityLimit)
        {
            // Lấy Flash Sale từ cơ sở dữ liệu
            var flashSale = await _context.FlashSales.FirstOrDefaultAsync(f => f.Id == flashSaleId);

            if (flashSale == null)
            {
                TempData["ErrorMessage"] = "Flash Sale không tồn tại!";
                return RedirectToAction("Index");
            }

            if (!flashSale.IsActive || flashSale.EndTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Flash Sale này đã kết thúc hoặc không còn hoạt động.";
                return RedirectToAction("Details", new { id = flashSaleId });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("Index");
            }

            if (!product.IsActive)
            {
                TempData["ErrorMessage"] = "Sản phẩm này không được phép thêm vào Flash Sale.";
                return RedirectToAction("Details", new { id = flashSaleId });
            }

            if (quantityLimit > product.Quantity)
            {
                TempData["ErrorMessage"] = $"Số lượng giới hạn cho Flash Sale ({quantityLimit}) vượt quá số lượng sản phẩm hiện có ({product.Quantity}).";
                return RedirectToAction("Details", new { id = flashSaleId });
            }


            var existing = await _context.FlashSaleProducts
                .FirstOrDefaultAsync(f => f.FlashSaleId == flashSaleId && f.ProductId == productId);

            if (existing == null)
            {
                decimal flashPriceRaw = product.Price * (1 - discountPercent / 100m);
                decimal flashPrice = Math.Round(flashPriceRaw / 1000m, MidpointRounding.AwayFromZero) * 1000;

                var flashSaleProduct = new FlashSaleProduct
                {
                    FlashSaleId = flashSaleId,
                    ProductId = productId,
                    FlashPrice = flashPrice,
                    QuantityLimit = quantityLimit
                };

                _context.FlashSaleProducts.Add(flashSaleProduct);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm sản phẩm vào Flash Sale thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Sản phẩm đã tồn tại trong Flash Sale.";
            }

            return RedirectToAction("Details", new { id = flashSaleId });
        }




        // POST: /FlashSales/RemoveProduct
        [HttpPost]
        public async Task<IActionResult> RemoveProduct(int flashSaleProductId)
        {
            var item = await _context.FlashSaleProducts.FindAsync(flashSaleProductId);
            if (item != null)
            {
                _context.FlashSaleProducts.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = item.FlashSaleId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var flashSale = await _context.FlashSales
                .Include(fs => fs.FlashSaleProducts)
                .FirstOrDefaultAsync(fs => fs.Id == id);

            if (flashSale == null)
            {
                return Json(new { success = false, message = "Không tìm thấy Flash Sale." });
            }

            _context.FlashSaleProducts.RemoveRange(flashSale.FlashSaleProducts);
            _context.FlashSales.Remove(flashSale);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã xoá Flash Sale \"{flashSale.Title}\" thành công!"
            });
        }


        [HttpGet]
        public IActionResult SearchProducts(string term)
        {
            var matchedProducts = _context.Products
                .Where(p => p.Name.Contains(term))
                .Select(p => new
                {
                    id = p.Id,
                    label = p.Name
                })
                .Take(10)
                .ToList();

            return Json(matchedProducts);
        }

        [HttpGet]
        public JsonResult GetProductPrice(int productId)
        {
            var product = _context.Products
                .Where(p => p.Id == productId)
                .Select(p => new {
                    issale = p.IsSale,        // Kiểm tra có đang bán hay không
                    price = p.Price,          // Giá gốc
                    pricesale = p.PriceSale,
                    quantity = p.Quantity,
                }).FirstOrDefault();

            return Json(product);
        }

        [HttpPost]
        public IActionResult UpdateActiveStatus([FromBody] UpdateStatusRequest data)
        {
            if (data == null)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var flashSale = _context.FlashSales.FirstOrDefault(f => f.Id == data.FlashSaleId);
            if (flashSale == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy Flash Sale.";
                return Json(new { success = false, message = "Không tìm thấy Flash Sale." });
            }

            // Nếu yêu cầu là bật hoạt động
            if (data.IsActive)
            {
                var now = DateTime.Now;
                if (flashSale.StartTime > now || flashSale.EndTime < now)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể kích hoạt Flash Sale khi chưa đến ngày bắt đầu hoặc đã kết thúc."
                    });
                }
            }

            flashSale.IsActive = data.IsActive;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công";
            return Json(new { success = true });
        }


        public class UpdateStatusRequest
        {
            public int FlashSaleId { get; set; }
            public bool IsActive { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> AddAllProducts(int flashSaleId, int discountPercent, int quantityLimit)
        {
            var flashSale = await _context.FlashSales.FirstOrDefaultAsync(f => f.Id == flashSaleId);
            if (flashSale == null || !flashSale.IsActive || flashSale.EndTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Flash Sale không hợp lệ hoặc đã kết thúc.";
                return RedirectToAction("Details", new { id = flashSaleId });
            }

            var allProducts = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            foreach (var product in allProducts)
            {
                bool exists = await _context.FlashSaleProducts
                    .AnyAsync(f => f.FlashSaleId == flashSaleId && f.ProductId == product.Id);

                if (!exists)
                {
                    decimal flashPriceRaw = product.Price * (1 - discountPercent / 100m);
                    decimal flashPrice = Math.Round(flashPriceRaw / 1000m, MidpointRounding.AwayFromZero) * 1000;
                    var finalQuantity = 0;
                    if(product.Quantity < quantityLimit)
                    {
                        finalQuantity = product.Quantity;
                    }
                    else
                    {
                        finalQuantity = quantityLimit;
                    }


                    _context.FlashSaleProducts.Add(new FlashSaleProduct
                    {
                        FlashSaleId = flashSaleId,
                        ProductId = product.Id,
                        FlashPrice = flashPrice,
                        QuantityLimit = finalQuantity
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm tất cả sản phẩm vào Flash Sale.";
            return RedirectToAction("Details", new { id = flashSaleId });
        }

        [HttpPost]
        public async Task<IActionResult> AddProductsByCategory(int flashSaleId, int categoryId, int discountPercent, int quantityLimit)
        {
            var flashSale = await _context.FlashSales.FirstOrDefaultAsync(f => f.Id == flashSaleId);
            if (flashSale == null || !flashSale.IsActive || flashSale.EndTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Flash Sale không hợp lệ hoặc đã kết thúc.";
                return RedirectToAction("Details", new { id = flashSaleId });
            }

            var products = await _context.Products
                .Where(p => p.ProductCategoryId == categoryId && p.IsActive)
                .ToListAsync();

            foreach (var product in products)
            {
                bool exists = await _context.FlashSaleProducts
                    .AnyAsync(f => f.FlashSaleId == flashSaleId && f.ProductId == product.Id);

                if (!exists)
                {
                    decimal flashPriceRaw = product.Price * (1 - discountPercent / 100m);
                    decimal flashPrice = Math.Round(flashPriceRaw / 1000m, MidpointRounding.AwayFromZero) * 1000;
                    var finalQuantity = 0;
                    if (product.Quantity < quantityLimit)
                    {
                        finalQuantity = product.Quantity;
                    }
                    else
                    {
                        finalQuantity = quantityLimit;
                    }
                    _context.FlashSaleProducts.Add(new FlashSaleProduct
                    {
                        FlashSaleId = flashSaleId,
                        ProductId = product.Id,
                        FlashPrice = flashPrice,
                        QuantityLimit = finalQuantity
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm các sản phẩm thuộc danh mục vào Flash Sale.";
            return RedirectToAction("Details", new { id = flashSaleId });
        }

        [HttpPost]
        public async Task<IActionResult> AddProductsBySubcategory(int flashSaleId, int subcategoryId, int discountPercent, int quantityLimit)
        {
            var flashSale = await _context.FlashSales.FirstOrDefaultAsync(f => f.Id == flashSaleId);
            if (flashSale == null || !flashSale.IsActive || flashSale.EndTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Flash Sale không hợp lệ hoặc đã kết thúc.";
                return RedirectToAction("Details", new { id = flashSaleId });
            }

            var products = await _context.Products
                .Where(p => p.ProductSubCategoryId == subcategoryId && p.IsActive)
                .ToListAsync();

            foreach (var product in products)
            {
                bool exists = await _context.FlashSaleProducts
                    .AnyAsync(f => f.FlashSaleId == flashSaleId && f.ProductId == product.Id);

                if (!exists)
                {
                    decimal flashPriceRaw = product.Price * (1 - discountPercent / 100m);
                    decimal flashPrice = Math.Round(flashPriceRaw / 1000m, MidpointRounding.AwayFromZero) * 1000;
                    var finalQuantity = 0;
                    if (product.Quantity < quantityLimit)
                    {
                        finalQuantity = product.Quantity;
                    }
                    else
                    {
                        finalQuantity = quantityLimit;
                    }
                    _context.FlashSaleProducts.Add(new FlashSaleProduct
                    {
                        FlashSaleId = flashSaleId,
                        ProductId = product.Id,
                        FlashPrice = flashPrice,
                        QuantityLimit = finalQuantity
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm các sản phẩm thuộc danh mục phụ vào Flash Sale.";
            return RedirectToAction("Details", new { id = flashSaleId });
        }

    }
}
