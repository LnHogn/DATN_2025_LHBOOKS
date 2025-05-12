using LHBooksWeb.Models.EF;

namespace LHBooksWeb.Models.ViewModels
{
    public class FlashSaleProductViewModel
    {
        public Product Product { get; set; }
        public decimal FlashPrice { get; set; }
        public int Sold { get; set; }
        public int QuantityLimit { get; set; }
        public int FlashSaleId {  get; set; }
    }


}
