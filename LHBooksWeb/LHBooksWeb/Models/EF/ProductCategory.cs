using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.EF
{
    [Table("tb_ProductCategory")]
    public class ProductCategory : BaseEntity
    {
        public ProductCategory()
        {
            this.Products = new HashSet<Product>();
            this.ProductSubCategories = new HashSet<ProductSubCategory>(); // Khởi tạo collection
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(150)]
        public string? Alias { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; }

        

        public ICollection<Product> Products { get; set; }
        public ICollection<ProductSubCategory> ProductSubCategories { get; set; }
    }
}
