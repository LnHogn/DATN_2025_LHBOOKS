using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace LHBooksWeb.Models.EF
{
    [Table("OrderDetail")]
    [PrimaryKey(nameof(OrderId), nameof(ProductId))]
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        // Thêm thành tiền để dễ dàng tính toán
        [NotMapped]
        public decimal SubTotal => Quantity * Price;

        // Lưu tên sản phẩm tại thời điểm đặt hàng (phòng khi sản phẩm bị thay đổi sau này)
        public string ProductName { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }


        public int? FlashSaleId { get; set; }
        public virtual FlashSale FlashSale { get; set; }
    }
}