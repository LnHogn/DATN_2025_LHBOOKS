using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LHBooksWeb.Data;

namespace LHBooksWeb.Models.EF
{
    [Table("ProductReview")]
    public class ProductReview : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;
    }

}
