using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using LHBooksWeb.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LHBooksWeb.Services
{
    // Services/OrderService.cs
    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;
        private readonly IEmailSender _emailSender;

        public OrderService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
            _emailSender = emailSender;
        }

        //public async Task<Order> CreateOrderAsync(int paymentMethod, string userId, OrderStatus status = OrderStatus.Pending)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        throw new Exception("Không tìm thấy thông tin người dùng");
        //    }

        //    var cartItems = await _cartService.GetCartItemsAsync();
        //    if (cartItems.Count == 0)
        //    {
        //        throw new Exception("Giỏ hàng trống");
        //    }

        //    var totalAmount = await _cartService.GetCartTotalAsync();
        //    var totalQuantity = cartItems.Sum(item => item.Quantity);

        //    var order = new Order
        //    {
        //        UserId = userId,
        //        CustomerName = user.FullName,
        //        Phone = user.PhoneNumber,
        //        Email = user.Email,
        //        Address = user.Address,
        //        OrderDate = DateTime.Now,
        //        TotalAmount = totalAmount,
        //        Quantity = totalQuantity,
        //        TypePayment = paymentMethod,
        //        Status = status,
        //        OrderDetails = new List<OrderDetail>()
        //    };

        //    // Truy vấn các FlashSale đang diễn ra
        //    var activeFlashSales = await _context.FlashSales
        //        .Include(fs => fs.FlashSaleProducts)
        //        .Where(fs => fs.StartTime <= DateTime.Now && fs.EndTime >= DateTime.Now)
        //        .ToListAsync();

        //    // Chuyển các item từ giỏ hàng sang chi tiết đơn hàng
        //    foreach (var item in cartItems)
        //    {
        //        var orderDetail = new OrderDetail
        //        {
        //            ProductId = item.ProductId,
        //            ProductName = item.ProductName,
        //            Price = item.Price,
        //            Quantity = item.Quantity
        //        };

        //        // Kiểm tra FlashSale cho sản phẩm trong giỏ hàng
        //        var flashSaleProduct = activeFlashSales
        //            .SelectMany(fs => fs.FlashSaleProducts, (fs, fsp) => new { fs, fsp })
        //            .FirstOrDefault(x => x.fsp.ProductId == item.ProductId);

        //        if (flashSaleProduct != null)
        //        {
        //            orderDetail.FlashSaleId = flashSaleProduct.fs.Id;
        //        }

        //        order.OrderDetails.Add(orderDetail);

        //        // Cập nhật số lượng tồn kho của sản phẩm
        //        var product = await _context.Products.FindAsync(item.ProductId);
        //        if (product == null)
        //        {
        //            throw new Exception($"Không tìm thấy sản phẩm với ID = {item.ProductId}");
        //        }

        //        if (product.Quantity < item.Quantity)
        //        {
        //            throw new Exception($"Sản phẩm '{product.Name}' không đủ hàng tồn kho.");
        //        }

        //        product.Quantity -= item.Quantity;
        //        _context.Products.Update(product);

        //        // Nếu sản phẩm nằm trong FlashSaleProduct thì cập nhật Sold
        //        if (flashSaleProduct != null)
        //        {
        //            flashSaleProduct.fsp.Sold += item.Quantity;
        //            _context.FlashSaleProducts.Update(flashSaleProduct.fsp);
        //        }
        //    }

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();


        //    return order;
        //}

        public async Task<Order> CreateOrderAsync(int paymentMethod, string userId, OrderStatus status = OrderStatus.Pending, decimal shippingFee = 0)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            if(user.Address == "Chưa có địa chỉ")
            {
                throw new Exception("Vui lòng nhập địa chỉ");
            }

            var cartItems = await _cartService.GetSelectedCartItemsAsync();
            if (cartItems.Count == 0)
            {
                throw new Exception("Giỏ hàng trống");
            }

            var totalAmount = cartItems.Sum(item => item.Price * item.Quantity);

            // Tính tổng tiền bao gồm phí ship
            var totalAmountWithShipping = totalAmount + shippingFee;
            var totalQuantity = cartItems.Sum(item => item.Quantity);

            var order = new Order
            {
                UserId = userId,
                CustomerName = user.FullName,
                Code = $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Phone = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmountWithShipping, // Cập nhật tổng tiền bao gồm phí ship
                ShippingFee = shippingFee, // Lưu phí ship vào đơn hàng
                Quantity = totalQuantity,
                TypePayment = paymentMethod,
                Status = status,
                OrderDetails = new List<OrderDetail>()
            };

            //Truy vấn các FlashSale đang diễn ra
               var activeFlashSales = await _context.FlashSales
                   .Include(fs => fs.FlashSaleProducts)
                   .Where(fs => fs.StartTime <= DateTime.Now && fs.EndTime >= DateTime.Now && fs.IsActive == true)
                   .ToListAsync();

            // Chuyển các item từ giỏ hàng sang chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity
                };

                // Kiểm tra FlashSale cho sản phẩm trong giỏ hàng
                var flashSaleProduct = activeFlashSales
                    .SelectMany(fs => fs.FlashSaleProducts, (fs, fsp) => new { fs, fsp })
                    .FirstOrDefault(x => x.fsp.ProductId == item.ProductId);

                if (flashSaleProduct != null)
                {
                    orderDetail.FlashSaleId = flashSaleProduct.fs.Id;
                }

                order.OrderDetails.Add(orderDetail);

                // Cập nhật số lượng tồn kho của sản phẩm
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    throw new Exception($"Không tìm thấy sản phẩm với ID = {item.ProductId}");
                }

                if (product.Quantity < item.Quantity)
                {
                    throw new Exception($"Sản phẩm '{product.Name}' không đủ hàng tồn kho.");
                }

                product.Quantity -= item.Quantity;
                _context.Products.Update(product);

                // Nếu sản phẩm nằm trong FlashSaleProduct thì cập nhật Sold
                if (flashSaleProduct != null)
                {
                    flashSaleProduct.fsp.Sold += item.Quantity;
                    _context.FlashSaleProducts.Update(flashSaleProduct.fsp);
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            //gui email

            if(paymentMethod == 1)
            {
                var receiver = user.Email;
                var subject = "Đặt hàng thành công - Mã đơn hàng: " + order.Code;

                // Tạo nội dung email chi tiết
                var messageBuilder = new System.Text.StringBuilder();
                messageBuilder.AppendLine("<h2>Xác nhận đơn hàng</h2>");
                messageBuilder.AppendLine("<p>Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi.</p>");
                messageBuilder.AppendLine("<p><strong>Mã đơn hàng:</strong> " + order.Code + "</p>");
                messageBuilder.AppendLine("<p><strong>Ngày đặt hàng:</strong> " + order.OrderDate.ToString("dd/MM/yyyy HH:mm:ss") + "</p>");

                // 1. Kiểm tra xem có sản phẩm nào trong đơn hàng thuộc Flash Sale không
                bool hasFlashSale = order.OrderDetails.Any(od => od.FlashSaleId != null);

                // 2. Xây dựng bảng HTML
                messageBuilder.AppendLine("<h3>Chi tiết đơn hàng:</h3>");
                messageBuilder.AppendLine("<table border='1' cellpadding='5' cellspacing='0' " +
                    "style='border-collapse: collapse; width: 80%; text-align: center;'>");
                messageBuilder.AppendLine("<tr>");
                messageBuilder.AppendLine("<th>Tên sản phẩm</th>");
                messageBuilder.AppendLine("<th>Giá</th>");
                messageBuilder.AppendLine("<th>Số lượng</th>");
                messageBuilder.AppendLine("<th>Thành tiền</th>");
                if (hasFlashSale)
                {
                    messageBuilder.AppendLine("<th>Flash Sale</th>");
                }
                messageBuilder.AppendLine("</tr>");

                // 3. Duyệt từng sản phẩm
                foreach (var detail in order.OrderDetails)
                {
                    messageBuilder.AppendLine("<tr>");
                    messageBuilder.AppendLine($"<td>{detail.ProductName}</td>");
                    messageBuilder.AppendLine($"<td>{detail.Price.ToString("N0")}₫</td>");
                    messageBuilder.AppendLine($"<td>{detail.Quantity}</td>");
                    messageBuilder.AppendLine($"<td>{(detail.Price * detail.Quantity).ToString("N0")}₫</td>");

                    if (hasFlashSale)
                    {
                        // Nếu đã include FlashSale, bạn có thể lấy tên
                        var flashSaleTitle = detail.FlashSale != null ? detail.FlashSale.Title : "Không";
                        messageBuilder.AppendLine($"<td>{flashSaleTitle}</td>");
                    }

                    messageBuilder.AppendLine("</tr>");
                }
                messageBuilder.AppendLine("</table>");

                messageBuilder.AppendLine($"<p><strong>Phí vận chuyển:</strong> {order.ShippingFee.ToString("N0")}₫</p>");
                messageBuilder.AppendLine($"<p><strong>Tổng thanh toán:</strong> {order.TotalAmount.ToString("N0")}₫</p>");
                messageBuilder.AppendLine($"<p><strong>Phương thức thanh toán:</strong> {(order.TypePayment == 1 ? "Thanh toán khi nhận hàng (COD)" : "Thanh toán online qua VNPAY")}</p>");
                messageBuilder.AppendLine("<p>Chúng tôi sẽ liên hệ với bạn sớm để xác nhận đơn hàng và tiến hành giao hàng.</p>");
                messageBuilder.AppendLine("<p>Trân trọng,</p>");
                messageBuilder.AppendLine("<p><strong>Đội ngũ hỗ trợ khách hàng</strong></p>");

                var message = messageBuilder.ToString();
                await _emailSender.SendEmailAsync(receiver, subject, message);
            }
            
            return order;
        }





        // Add this to your OrderService
        public async Task UpdateOrderPaymentStatusAsync(int orderId, int isPaid)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.TypePayment = isPaid;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders.FindAsync(orderId);
        }

    }
}
