using LHBooksWeb.Models.EF;
using LHBooksWeb.Services.ProductHome;
using Microsoft.AspNetCore.Mvc;
using static LHBooksWeb.Common.Common;

namespace LHBooksWeb.Controllers
{
    public class BooksByCateController : Controller
    {
        private readonly IProductService _productService;

        public BooksByCateController(IProductService productService)
        {
            _productService = productService;
        }


        public IActionResult ViewMore(string type, bool showAll = false)
        {
            BookViewType viewType;
            if (!Enum.TryParse(type, true, out viewType))
            {
                return NotFound();
            }

            IEnumerable<Product> products;
            string title = "", keyword = "";

            switch (viewType)
            {
                case BookViewType.Featured:
                    products = _productService.GetFeaturedBooks(showAll);
                    title = "Sách Nổi Bật";
                    keyword = "Sách Nổi Bật";
                    break;
                case BookViewType.Hot:
                    products = _productService.GetHotBooks(showAll);
                    title = "Sách Hot";
                    keyword = "Sách Hot";
                    break;
                case BookViewType.BestSeller:
                    products = _productService.GetBestSellerBooks(showAll);
                    title = "Xu hướng mua sắm";
                    keyword = "Xu hướng mua sắm";
                    break;
                case BookViewType.Sale:
                    products = _productService.GetSaleBooks(showAll);
                    title = "Sách Khuyến Mại";
                    keyword = "Sách Khuyến Mại";
                    break;
                case BookViewType.NewRelease:
                    products = _productService.GetNewBooks(showAll);
                    title = "Sách Mới Phát Hành";
                    keyword = "Sách Mới Phát Hành";
                    break;
                default:
                    products = new List<Product>();
                    break;
            }

            ViewData["Title"] = title;
            ViewBag.Keyword = keyword;
            ViewBag.ShowAll = showAll; // Thêm biến để biết trạng thái hiện tại
            ViewBag.Type = type; // Truyền type để tạo URL xem tất cả

            return View(products);
        }
    }
}
