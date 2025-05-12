using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.ViewModels
{
    public class ProductReviewViewModel
    {
        public int ProductId { get; set; }
        public string Alias { get; set; }


        [Range(1, 5, ErrorMessage = "Vui lòng chọn số sao từ 1 đến 5.")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
    }

}
