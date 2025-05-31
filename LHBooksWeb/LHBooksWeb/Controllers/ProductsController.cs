using DocumentFormat.OpenXml.Spreadsheet;
using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using LHBooksWeb.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Products

        public async Task<IActionResult> Index(string sort, int publisherId = 0)
        {
            // Khởi tạo query ban đầu
            var query = _context.Products
                .Include(p => p.ProductImage)
                .Include(p => p.Publisher)
                .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                .AsQueryable();


            // Sau đó mới áp dụng lọc theo nhà xuất bản
            if (publisherId > 0)
            {
                query = query.Where(p => p.PublisherId == publisherId);
            }

            // Áp dụng sắp xếp (nếu có)
            switch (sort)
            {
                case "price":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "name":
                    query = query.OrderBy(p => p.Name);
                    break;
                case "mostViewed":
                    query = query.OrderByDescending(p => p.ViewCount);
                    break;
                default:
                    query = query.OrderBy(p => p.Id); // mặc định
                    break;
            }

            var items = await query.ToListAsync();

            // Lấy danh sách nhà xuất bản để hiển thị trong dropdown
            var publishers = await _context.Publishers
                .OrderBy(p => p.Name)
                .ToListAsync(); // Truyền trực tiếp đối tượng Publisher đầy đủ

            ViewBag.Publishers = publishers;


            // Nếu request là AJAX, trả về partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductListPartial", items);
            }

            return View(items);
        }

        public async Task<IActionResult> Search(string search)
        {
            ViewBag.SearchKeyword = search;

            if (string.IsNullOrWhiteSpace(search))
            {
                return RedirectToAction("Index", "Products");
            }

            string lowered = search.ToLower();

            var products = await _context.Products
                .Include(p => p.ProductImage)
                .Include(p => p.Publisher)
                .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                .Where(p =>
                    EF.Functions.Like(p.Name.ToLower(), $"%{lowered}%") ||
                    EF.Functions.Like(p.AuthorName.ToLower(), $"%{lowered}%"))
                .ToListAsync();

            if (!products.Any())
            {
                ViewBag.NotFoundMessage = "Không tìm thấy sản phẩm phù hợp với từ khóa tìm kiếm.";
            }

            return View(products);
        }




        // GET: /Products/Detail/alias/1
        public async Task<IActionResult> Detail(string alias, int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductSubCategory)
                .Include(p => p.Publisher)
                .Include(p => p.ProductImage)
                .Include(p => p.FlashSaleProducts)
                .ThenInclude(fsp => fsp.FlashSale)
                .FirstOrDefaultAsync(p => p.Id == id && p.Alias == alias && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            var latestReviews = await _context.productReviews
                .Where(r => r.ProductId == product.Id)
                .OrderByDescending(r => r.CreatedDate) // hoặc thời gian tương ứng
                .Include(r => r.User)
                .Take(20)
                .ToListAsync();

            product.ProductReviews = latestReviews;


            // Increment view count
            if (product.ViewCount.HasValue)
            {
                product.ViewCount++;
            }
            else
            {
                product.ViewCount = 1;
            }

            // Tính số sao trung bình
            double averageRating = 0;
            int reviewCount = 0;

            if (product.ProductReviews != null && product.ProductReviews.Any())
            {
                averageRating = product.ProductReviews.Average(r => r.Rating);
                reviewCount = product.ProductReviews.Count();
            }

            // Truyền số sao trung bình và số lượng đánh giá vào ViewBag
            ViewBag.AverageRating = averageRating;
            ViewBag.ReviewCount = reviewCount;

            // Lấy số lượng đã bán
            var soldQuantity = await _context.OrderDetails
                .Where(od => od.ProductId == id &&
                             (od.Order.Status == OrderStatus.Delivered))
                .SumAsync(od => (int?)od.Quantity) ?? 0;

            ViewBag.SoldQuantity = soldQuantity;

            

            // Tìm flash sale đang diễn ra cho sản phẩm hiện tại
            var flashSale = product.FlashSaleProducts
                .FirstOrDefault(fsp => fsp.FlashSale != null && fsp.FlashSale.EndTime > DateTime.Now && fsp.FlashSale.IsActive == true);

            bool isInFlashSale = flashSale != null;
            bool isFlashSaleSoldOut = isInFlashSale && flashSale.Sold >= flashSale.QuantityLimit;

            if (isInFlashSale)
            {
                // Số lượng còn lại trong Flash Sale
                ViewBag.StockQuantity = flashSale.QuantityLimit - flashSale.Sold;
            }
            else
            {
                // Số lượng còn lại thông thường
                ViewBag.StockQuantity = product.Quantity;
            }

            await _context.SaveChangesAsync();
            return View(product);
        }

       
        public IActionResult FlashSaleAll(int flashSaleId)
        {
            var flashSale = _context.FlashSales.FirstOrDefault(f => f.Id == flashSaleId);
            if (flashSale == null) return NotFound();

            var products = _context.FlashSaleProducts
                .Include(f => f.Product)
                .Where(f => f.FlashSaleId == flashSaleId)
                .ToList();

            ViewBag.FlashSaleName = flashSale.Title;
            return View("FlashSaleAll", products); // View tên FlashSaleAll.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(ProductReviewViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đánh giá sản phẩm.";
                return RedirectToAction("Detail", new { alias = model.Alias, id = model.ProductId });
            }

            var userId = _userManager.GetUserId(User);

            // Kiểm tra xem người dùng đã đánh giá sản phẩm này chưa
            var existingReview = await _context.productReviews
                .FirstOrDefaultAsync(r => r.ProductId == model.ProductId && r.UserId == userId);

            if (existingReview != null)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này rồi.";
                return RedirectToAction("Detail", new { alias = model.Alias, id = model.ProductId });
            }

            if (ModelState.IsValid)
            {
                var review = new ProductReview
                {
                    ProductId = model.ProductId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    IsApproved = true,
                    CreatedDate = DateTime.Now
                };

                _context.productReviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đánh giá của bạn đã được ghi nhận.";
                return RedirectToAction("Detail", new { alias = model.Alias, id = model.ProductId });
            }

            TempData["Error"] = "Đánh giá không hợp lệ.";
            return RedirectToAction("Detail", new { alias = model.Alias, id = model.ProductId });
        }




        //public async Task<IActionResult> RelatedProducts(int subCategoryId, string authorName, int currentProductId)
        //{
        //    var relatedProducts = await _context.Products
        //        .Where(p =>
        //            p.IsActive &&
        //            p.Id != currentProductId &&
        //            (p.ProductSubCategoryId == subCategoryId || p.AuthorName == authorName)
        //        )
        //        .Include(p => p.ProductImage)
        //        .Include(p => p.Publisher)
        //        .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
        //        .OrderByDescending(p => p.ViewCount)
        //        .Take(6)
        //        .ToListAsync();


        //    return PartialView("_RelatedProducts", relatedProducts);
        //}

        public async Task<IActionResult> RelatedProducts(int subCategoryId, string authorName, int currentProductId)
        {
            var user = await _userManager.GetUserAsync(User);
            List<Product> relatedProducts;

            if (user != null)
            {
                // Lấy danh sách các sản phẩm người dùng đã mua
                var purchasedProductIds = await _context.OrderDetails
                    .Where(od => od.Order.UserId == user.Id)
                    .Select(od => od.ProductId)
                    .ToListAsync();

                // Lấy các thể loại con và tác giả người dùng mua nhiều nhất
                var topSubCategories = await _context.OrderDetails
                    .Where(od => od.Order.UserId == user.Id)
                    .GroupBy(od => od.Product.ProductSubCategoryId)
                    .OrderByDescending(g => g.Sum(x => x.Quantity))
                    .Select(g => g.Key)
                    .Take(2)
                    .ToListAsync();

                var topAuthors = await _context.OrderDetails
                    .Where(od => od.Order.UserId == user.Id)
                    .GroupBy(od => od.Product.AuthorName)
                    .OrderByDescending(g => g.Sum(x => x.Quantity))
                    .Select(g => g.Key)
                    .Take(2)
                    .ToListAsync();

                relatedProducts = await _context.Products
                    .Where(p =>
                        p.IsActive &&
                        p.Id != currentProductId &&
                        (
                            p.ProductSubCategoryId == subCategoryId ||
                            p.AuthorName == authorName ||
                            topSubCategories.Contains(p.ProductSubCategoryId) ||
                            topAuthors.Contains(p.AuthorName)
                        )
                    )
                    .Include(p => p.ProductImage)
                    .Include(p => p.Publisher)
                    .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                    .OrderByDescending(p => p.ViewCount)
                    .Take(6)
                    .ToListAsync();
            }
            else
            {
                // Logic cũ cho người chưa đăng nhập
                relatedProducts = await _context.Products
                    .Where(p =>
                        p.IsActive &&
                        p.Id != currentProductId &&
                        (p.ProductSubCategoryId == subCategoryId || p.AuthorName == authorName)
                    )
                    .Include(p => p.ProductImage)
                    .Include(p => p.Publisher)
                    .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                    .OrderByDescending(p => p.ViewCount)
                    .Take(6)
                    .ToListAsync();
            }

            return PartialView("_RelatedProducts", relatedProducts);
        }

    }
}
