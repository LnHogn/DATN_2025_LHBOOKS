using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace LHBooksWeb.Models.EF
{
    [Table("tb_Order")]
    public class Order : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mã đơn hàng")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Tên khách hàng không để trống")]
        [Display(Name = "Khách hàng")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Địa chỉ không để trống")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Tổng tiền")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public int TypePayment { get; set; }

        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; }

        // Thêm ngày đặt hàng
        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; }

        // Thêm ID người dùng nếu có hệ thống đăng nhập
        public string UserId { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        [Display(Name = "Phí ship")]
        [DataType(DataType.Currency)]
        public decimal ShippingFee { get; set; }


        public Order()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
            this.OrderDate = DateTime.Now;
            this.Code = GenerateOrderCode();
        }

        private string GenerateOrderCode()
        {
            // Tạo mã đơn hàng tự động theo format: ORD-yyyyMMdd-xxxx (xxxx là số ngẫu nhiên)
            return $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }
    }

    public enum OrderStatus
    {
        [Display(Name = "Chờ thanh toán")]
        AwaitingPayment = 0,

        [Display(Name = "Thanh toán thất bại")]
        PaymentFailed = 1,

        [Display(Name = "Chờ xác nhận")]
        Pending = 2,

        [Display(Name = "Đã xác nhận")]
        Confirmed = 3,

        [Display(Name = "Đang giao hàng")]
        Shipping = 4,

        [Display(Name = "Đã giao hàng")]
        Delivered = 5,

        [Display(Name = "Đã hủy")]
        Cancelled = 6,

            [Display(Name = "Đã thanh toán")]
        Paid = 7
    }


}
