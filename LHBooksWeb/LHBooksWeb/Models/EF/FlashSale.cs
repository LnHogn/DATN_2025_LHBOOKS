using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.EF
{
    public class FlashSale : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<FlashSaleProduct> FlashSaleProducts { get; set; } = new HashSet<FlashSaleProduct>();
    }

}
