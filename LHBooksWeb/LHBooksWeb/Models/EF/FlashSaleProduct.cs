using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.EF
{
        public class FlashSaleProduct
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public int FlashSaleId { get; set; }
            public virtual FlashSale FlashSale { get; set; }

            [Required]
            public int ProductId { get; set; }
            public virtual Product Product { get; set; }

            [Required]
            public decimal FlashPrice { get; set; }

            public int QuantityLimit { get; set; } = 0; // giới hạn số lượng trong đợt sale

            public int Sold { get; set; } = 0; // số lượng đã bán
        }

}
