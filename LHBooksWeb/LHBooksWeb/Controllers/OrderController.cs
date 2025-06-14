﻿using LHBooksWeb.Common;
using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using LHBooksWeb.Models.Vnpay;
using LHBooksWeb.Services;
using LHBooksWeb.Services.Email;
using LHBooksWeb.Services.Vnpay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using IEmailSender = LHBooksWeb.Services.Email.IEmailSender;

namespace LHBooksWeb.Controllers
{
    // Controllers/OrderController.cs
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVnPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        public OrderController(OrderService orderService, UserManager<ApplicationUser> userManager, IVnPayService vnPayService, ApplicationDbContext context, CartService cartService, IEmailSender emailSender)
        {
            _orderService = orderService;
            _userManager = userManager;
            _vnPayService = vnPayService;
            _context = context;
            _cartService = cartService;
            _emailSender = emailSender;
        }


        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy thông tin người dùng từ AspNetUsers
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin người dùng.");
                return RedirectToAction("Checkout");
            }

            // Lấy địa chỉ của người dùng
            var address = user.Address;

            // Lấy giỏ hàng
            var cartItems = await _cartService.GetSelectedCartItemsAsync();
            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] = "Hãy chọn sản phẩm cần mua";
                return RedirectToAction("Index","Cart");
            }

            var totalAmount = cartItems.Sum(item => item.Price * item.Quantity);

            // Tính phí ship
            var shippingFee = CalculateShippingFee(address);  // Sử dụng hàm tính phí ship với địa chỉ
            var totalAmountWithShipping = totalAmount + shippingFee;

            // Truyền giá trị vào view
            ViewBag.ShippingFee = shippingFee;
            ViewBag.TotalAmountWithShipping = totalAmountWithShipping;

            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int paymentMethod)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                // Lấy giỏ hàng
                var cartItems = await _cartService.GetSelectedCartItemsAsync();
                if (cartItems == null || !cartItems.Any())
                {
                    ModelState.AddModelError("", "Giỏ hàng của bạn đang trống.");
                    return RedirectToAction("Checkout");
                }

                // Tính tổng tiền cho sản phẩm
                var totalAmount = cartItems.Sum(item => item.Price * item.Quantity);

                // Tính phí ship (có thể thay đổi tùy theo logic bạn muốn)
                var shippingFee = CalculateShippingFee(user.Address);
                var totalAmountWithShipping = totalAmount + shippingFee;

                if (paymentMethod == 1) // COD
                {
                    // Tạo đơn hàng với phí ship đã được tính
                    var order = await _orderService.CreateOrderAsync(paymentMethod, userId, OrderStatus.Pending, shippingFee);

                    // Xóa giỏ hàng sau khi tạo đơn
                    await _cartService.RemoveSelectedItemsAsync();

                    return RedirectToAction("OrderConfirmation");
                }
                else if (paymentMethod == 2) // VNPay
                {
                    // Bước 1: Tạo đơn hàng với trạng thái Chờ thanh toán và tính phí ship
                    var order = await _orderService.CreateOrderAsync(paymentMethod, userId, OrderStatus.AwaitingPayment, shippingFee);

                    // Bước 2: Gửi thông tin sang VNPay
                    var paymentModel = new PaymentInformationModel
                    {
                        OrderId = order.Id,
                        Amount = (decimal)totalAmountWithShipping,
                        OrderDescription = $"Thanh toán đơn hàng #{order.Id} lúc {DateTime.Now:yyyyMMddHHmmss}",
                        Name = User.Identity?.Name
                    };

                    var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
                    return Redirect(paymentUrl);
                }

                ModelState.AddModelError("", "Phương thức thanh toán không hợp lệ.");
                return RedirectToAction("Checkout");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Checkout");
            }

        }



        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            // Hiển thị thông tin xác nhận đơn hàng
            return View(orderId);
        }


        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            // Lấy OrderId từ VNPay (mặc định VNPay dùng vnp_TxnRef để truyền OrderId)
            if (!int.TryParse(Request.Query["vnp_TxnRef"], out int orderId))
            {
                TempData["Message"] = "Không tìm thấy mã đơn hàng hợp lệ.";
                return RedirectToAction("PaymentFail");
            }

            var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.FlashSale) // nếu muốn lấy thông tin FlashSale
                    .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["Message"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("PaymentFail");
            }

            // Nếu thanh toán thất bại
            if (response.VnPayResponseCode != "00")
            {
                order.Status = OrderStatus.PaymentFailed;
                order.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                string errorMessage = VnPayErrorDictionary.ErrorMessages.TryGetValue(response.VnPayResponseCode, out string message)
    ? message
    : $"Giao dịch không thành công : {response.VnPayResponseCode}";

                TempData["Message"] = errorMessage;
                return RedirectToAction("PaymentFail");
            }

            // Nếu thanh toán thành công
            order.Status = OrderStatus.Paid;
            order.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync(); // lưu thay đổi vào DB
            await _cartService.RemoveSelectedItemsAsync();

            var user = await _userManager.FindByIdAsync(order.UserId);
            if (user != null)
            {
                var receiver = user.Email;
                var subject = "Đặt hàng thành công - Mã đơn hàng: " + order.Code;

                var messageBuilder = new System.Text.StringBuilder();
                messageBuilder.AppendLine("<h2>Xác nhận đơn hàng</h2>");
                messageBuilder.AppendLine("<p>Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi.</p>");
                messageBuilder.AppendLine("<p><strong>Mã đơn hàng:</strong> " + order.Code + "</p>");
                messageBuilder.AppendLine("<p><strong>Ngày đặt hàng:</strong> " + order.OrderDate.ToString("dd/MM/yyyy HH:mm:ss") + "</p>");

                bool hasFlashSale = order.OrderDetails.Any(od => od.FlashSaleId != null);

                messageBuilder.AppendLine("<h3>Chi tiết đơn hàng:</h3>");
                messageBuilder.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 80%; text-align: center;'>");
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

                foreach (var detail in order.OrderDetails)
                {
                    messageBuilder.AppendLine("<tr>");
                    messageBuilder.AppendLine($"<td>{detail.ProductName}</td>");
                    messageBuilder.AppendLine($"<td>{detail.Price.ToString("N0")}₫</td>");
                    messageBuilder.AppendLine($"<td>{detail.Quantity}</td>");
                    messageBuilder.AppendLine($"<td>{(detail.Price * detail.Quantity).ToString("N0")}₫</td>");
                    if (hasFlashSale)
                    {
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

            TempData["Message"] = "Thanh toán VnPay thành công, đơn hàng đã được xác nhận.";
            return RedirectToAction("OrderConfirmation");
        }


        public IActionResult PaymentFail()
        {
            return View();
        }


        public decimal CalculateShippingFee(string address)
        {
            // Chuyển toàn bộ địa chỉ thành chữ in thường để dễ tìm kiếm
            address = address.ToLower();

            // Kiểm tra xem địa chỉ có chứa các từ khóa của các khu vực
            if (address.Contains("hà nội"))
            {
                return 20000; // Phí ship ở Hà Nội
            }
            else if (address.Contains("hà giang") || address.Contains("cao bằng") || address.Contains("bắc kạn") || address.Contains("lạng sơn") ||
                     address.Contains("tuyên quang") || address.Contains("thái nguyên") || address.Contains("phú thọ") || address.Contains("bắc giang") ||
                     address.Contains("quảng ninh") || address.Contains("hải phòng") || address.Contains("hải dương") || address.Contains("hưng yên") ||
                     address.Contains("vĩnh phúc") || address.Contains("bắc ninh") || address.Contains("thái bình") || address.Contains("nam định") ||
                     address.Contains("hà nam") || address.Contains("ninh bình"))
            {
                return 25000; // Phí ship ở miền Bắc (Ngoài Hà Nội)
            }
            else if (address.Contains("thành phố hồ chí minh") || address.Contains("cần thơ") ||
                     address.Contains("bình dương") || address.Contains("bình phước") || address.Contains("tây ninh") || address.Contains("bà rịa – vũng tàu") ||
                     address.Contains("đồng nai") || address.Contains("long an") || address.Contains("tiền giang") || address.Contains("bến tre") ||
                     address.Contains("vĩnh long") || address.Contains("trà vinh") || address.Contains("đồng tháp") || address.Contains("hậu giang") ||
                     address.Contains("an giang") || address.Contains("kiên giang") || address.Contains("bạc liêu") || address.Contains("sóc trăng") ||
                     address.Contains("cà mau"))
            {
                return 35000; // Phí ship ở miền Nam
            }
            else
            {
                return 30000; // Phí ship ở miền Trung (các tỉnh còn lại)
            }
        }

    }
}
