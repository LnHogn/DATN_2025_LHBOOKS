using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã sản phẩm")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(250, ErrorMessage = "Tên sản phẩm không được vượt quá 250 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả ngắn")]
        public string Description { get; set; }

        [DisplayFormat(HtmlEncode = true)]
        [Required(ErrorMessage = "Vui lòng nhập chi tiết sản phẩm")]
        public string Detail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal PriceSale { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int ProductCategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục con")]
        public int ProductSubCategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tác giả")]
        public string? AuthorName { get; set; }

        public int PublisherId { get; set; }

        [BindNever]
        public string? PublisherName { get; set; }

        public string? Alias { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsHome { get; set; }
        public bool IsHot { get; set; }
        public bool IsSale { get; set; }
        public bool IsFeature { get; set; }

        // Thuộc tính để xử lý upload nhiều file
        public List<IFormFile>? ImageFiles { get; set; }

        // Id của ảnh được chọn làm mặc định
        public int DefaultImageId { get; set; }

        [Range(1000, 2200, ErrorMessage = "Năm xuất bản không hợp lệ")]
        [Display(Name = "Năm xuất bản")]
        [Required(ErrorMessage = "Vui lòng nhập năm xuất bản")]
        public int? PublishYear { get; set; }

        [StringLength(100, ErrorMessage = "Ngôn ngữ không được vượt quá 100 ký tự")]
        [Display(Name = "Ngôn ngữ")]
        [Required(ErrorMessage = "Vui lòng nhập ngôn ngữ")]
        public string? Language { get; set; }

        [Range(0, 100000, ErrorMessage = "Trọng lượng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Trọng lượng (gr)")]
        [Required(ErrorMessage = "Vui lòng nhập trọng lượng")]
        public int? Weight { get; set; }

        [Range(0, 10000, ErrorMessage = "Số trang phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số trang")]
        [Required(ErrorMessage = "Vui lòng nhập số trang")]
        public int? PageCount { get; set; }

        [StringLength(100, ErrorMessage = "Hình thức không được vượt quá 100 ký tự")]
        [Display(Name = "Hình thức")]
        public string? Format { get; set; }

        [StringLength(100, ErrorMessage = "Người dịch không được vượt quá 100 ký tự")]
        [Display(Name = "Người dịch")]
        public string? Translator { get; set; }
    }

}
