using LHBooksWeb.Models.EF;
using System.Drawing;

namespace LHBooksWeb.Models
{
    public class ShoppingCartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total
        {
            get { return Price * Quantity; }
        }
        public string Image {  get; set; }
            
        public ShoppingCartItem() { }

        public ShoppingCartItem(Product product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Quantity = 1;
            Price = product.PriceSale;
            Image = product.Image;
        }
    }
}
