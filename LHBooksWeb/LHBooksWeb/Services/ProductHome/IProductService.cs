using LHBooksWeb.Models.EF;

namespace LHBooksWeb.Services.ProductHome
{
    //public interface IProductService
    //{
    //    IEnumerable<Product> GetFeaturedBooks();
    //    IEnumerable<Product> GetHotBooks();
    //    IEnumerable<Product> GetBestSellerBooks();
    //    IEnumerable<Product> GetSaleBooks();
    //    IEnumerable<Product> GetNewBooks();
    //}

    public interface IProductService
    {
        IEnumerable<Product> GetFeaturedBooks(bool all = false);
        IEnumerable<Product> GetHotBooks(bool all = false);
        IEnumerable<Product> GetBestSellerBooks(bool all = false);
        IEnumerable<Product> GetSaleBooks(bool all = false);
        IEnumerable<Product> GetNewBooks(bool all = false);
    }
}
