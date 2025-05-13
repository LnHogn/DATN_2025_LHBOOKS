using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.EF
{
    [Table("Publisher")]
    public class Publisher : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public Publisher()
        {
            Products = new HashSet<Product>();
        }
    }
}
