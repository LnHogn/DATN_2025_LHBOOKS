using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LHBooksWeb.Models.EF
{
    [Table("tb_CartItem")]
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; }

        public decimal Price { get; set; }

        public string ProductName { get; set; }

        public string ProductImage { get; set; }

        public decimal SubTotal => Quantity * Price;

        public virtual Product Product { get; set; }
    }
}