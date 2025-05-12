using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Services.ProductHome
{
    //public class ProductService : IProductService
    //{
    //    private readonly ApplicationDbContext _context;

    //    public ProductService(ApplicationDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public IEnumerable<Product> GetFeaturedBooks()
    //    {
    //        return _context.Products
    //            .Where(p => p.IsFeature && p.IsActive)
    //            .OrderByDescending(p => p.PublishedDate)
    //            .Take(20)
    //            .ToList();
    //    }

    //    public IEnumerable<Product> GetHotBooks()
    //    {
    //        return _context.Products
    //            .Where(p => p.IsHot && p.IsActive)
    //            .OrderByDescending(p => p.ViewCount ?? 0)
    //            .Take(20)
    //            .ToList();
    //    }

    //    public IEnumerable<Product> GetBestSellerBooks()
    //    {
    //        return _context.Products
    //            .Where(p => p.IsActive)
    //            .OrderByDescending(p => p.OrderDetails.Sum(od => od.Quantity))
    //            .Take(20)
    //            .ToList();
    //    }

    //    public IEnumerable<Product> GetSaleBooks()
    //    {
    //        return _context.Products
    //            .Where(p => p.IsSale && p.IsActive && p.PriceSale < p.Price)
    //            .OrderByDescending(p => (p.Price - p.PriceSale))
    //            .Take(20)
    //            .ToList();
    //    }

    //    public IEnumerable<Product> GetNewBooks()
    //    {
    //        return _context.Products
    //            .Where(p => p.IsActive && p.PublishedDate != null)
    //            .OrderByDescending(p => p.PublishedDate)
    //            .Take(20)
    //            .ToList();
    //    }
    //}
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetFeaturedBooks(bool all = false)
        {
            var query = _context.Products
                .Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                .Where(p => p.IsFeature && p.IsActive)
                .OrderByDescending(p => p.PublishedDate);

            return all ? query.ToList() : query.Take(20).ToList();
        }

        public IEnumerable<Product> GetHotBooks(bool all = false)
        {
            var query = _context.Products.Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)

                .Where(p => p.IsHot && p.IsActive)
                .OrderByDescending(p => p.ViewCount ?? 0);

            return all ? query.ToList() : query.Take(20).ToList();
        }

        public IEnumerable<Product> GetBestSellerBooks(bool all = false)
        {
            var query = _context.Products.Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.OrderDetails.Sum(od => od.Quantity));

            return all ? query.ToList() : query.Take(20).ToList();
        }

        public IEnumerable<Product> GetSaleBooks(bool all = false)
        {
            var query = _context.Products.Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                .Where(p => p.IsSale && p.IsActive && p.PriceSale < p.Price)
                .OrderByDescending(p => (p.Price - p.PriceSale));

            return all ? query.ToList() : query.Take(20).ToList();
        }

        public IEnumerable<Product> GetNewBooks(bool all = false)
        {
            var query = _context.Products.Include(p => p.FlashSaleProducts).ThenInclude(fsp => fsp.FlashSale)
                .Where(p => p.IsActive && p.PublishedDate != null)
                .OrderByDescending(p => p.PublishedDate);

            return all ? query.ToList() : query.Take(20).ToList();
        }
    }
}
