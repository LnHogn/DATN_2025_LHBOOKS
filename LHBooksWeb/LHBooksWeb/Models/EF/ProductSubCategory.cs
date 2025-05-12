using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.EF
{
    [Table("tb_ProductSubCategory")]
    public class ProductSubCategory : BaseEntity
    {
        public ProductSubCategory()
        {
            this.Products = new HashSet<Product>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(150)]
        public string? Alias { get; set; }

        public string Description { get; set; }

        

        // Quan hệ với ProductCategory (Danh mục chính)
        [ForeignKey("ProductCategory")]
        public int ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; }

        // Quan hệ với Products
        public ICollection<Product> Products { get; set; }
    }
}
