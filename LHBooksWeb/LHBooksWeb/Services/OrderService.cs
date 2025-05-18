using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
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

        public OrderService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
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
