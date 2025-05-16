using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.EF
{
    public class FlashSale : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Tiêu đề không được để trống.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống.")]
        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<FlashSaleProduct> FlashSaleProducts { get; set; } = new HashSet<FlashSaleProduct>();
    }

}
