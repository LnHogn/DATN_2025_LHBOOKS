using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.EF
{
    [Table("tb_Product")]
    public class Product : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ProductCode { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        public string Description { get; set; }

        [DisplayFormat(HtmlEncode = true)]
        public string Detail { get; set; }

        public string Image { get; set; }

        public decimal Price { get; set; }

        public decimal PriceSale { get; set; }

        public int Quantity { get; set; }

        public int? PublishYear { get; set; } // Năm XB

        [StringLength(100)]
        public string? Language { get; set; } // Ngôn Ngữ

        public int? Weight { get; set; } // Trọng lượng (gr)

        public int? PageCount { get; set; } // Số trang

        [StringLength(100)]
        public string? Format { get; set; }

        public string? Translator { get; set; }

        public int ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }

        public int ProductSubCategoryId { get; set; }
        public virtual ProductSubCategory ProductSubCategory { get; set; }

        public string AuthorName { get; set; }

        //nxb
        public int PublisherId { get; set; }
        public virtual Publisher Publisher { get; set; }

        public string? Alias { get; set; }
        public bool IsActive { get; set; }

        public bool IsHot { get; set; }
        public bool IsSale { get; set; }
        public bool IsFeature { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PublishedDate { get; set; }

        public int? ViewCount { get; set; }

        public virtual ICollection<ProductImage> ProductImage { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<FlashSaleProduct> FlashSaleProducts { get; set; }
        public virtual ICollection<ProductReview> ProductReviews { get; set; }

        public Product()
        {
            this.ProductImage = new HashSet<ProductImage>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.FlashSaleProducts = new HashSet<FlashSaleProduct>();
            this.ProductReviews = new HashSet<ProductReview>();

        }
    }

}
