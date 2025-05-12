using LHBooksWeb.Data;
using LHBooksWeb.Models.EF;
using LHBooksWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Image;
using System.IO;
using LHBooksWeb.Common;
using DocumentFormat.OpenXml.Presentation;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using iText.IO.Font;
using static iText.Kernel.Font.PdfFontFactory;
using iText.Kernel.Geom;
using Path = System.IO.Path;

namespace LHBooksWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard view showing all key statistics
        public async Task<IActionResult> Index()
        {
            var dashboardVM = new DashboardViewModel
            {
                TotalSales = await GetTotalSales(),
                TotalOrders = await GetTotalOrders(),
                NewOrders = await GetNewOrdersCount(),
                TopSellingProducts = await GetTopSellingProducts(5),
                MonthlySalesData = await GetMonthlySalesData(),
                OrderStatusDistribution = await GetOrderStatusDistribution(),
                ActiveFlashSaleCount = await GetActiveFlashSaleCount(),
                TotalProducts = await GetTotalProducts(),
            };

            return View(dashboardVM);
        }

        // Orders Statistics


        public async Task<IActionResult> OrderStatistics(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate?.AddDays(1).AddMilliseconds(-1) ?? DateTime.Now;

            var ordersInRange = _context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate <= end);

            var orderStats = new OrderStatisticsViewModel
            {
                StartDate = start,
                EndDate = end,
                TotalOrders = await ordersInRange.CountAsync(),
                CompletedOrders = await ordersInRange.CountAsync(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = await ordersInRange.CountAsync(o => o.Status == OrderStatus.Cancelled),
                PendingOrders = await ordersInRange.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.AwaitingPayment),
                ShippingOrders = await ordersInRange.CountAsync(o => o.Status == OrderStatus.Shipping),
                ConfirmeOrder = await ordersInRange.CountAsync(o => o.Status == OrderStatus.Confirmed),
                PaymentFailed = await ordersInRange.CountAsync(o => o.Status == OrderStatus.PaymentFailed),
                AverageOrderValue = await ordersInRange
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .AverageAsync(o => o.TotalAmount),
                OrdersByDate = await GetOrdersByDate(start, end) // Gửi khoảng thời gian vào
            };

            return View(orderStats);
        }

        // Product Statistics
        public async Task<IActionResult> ProductStatistics()
        {
            var outOfStockList = await _context.Products
    .Where(p => p.IsActive && p.Quantity <= 0)
    .Select(p => new OutOfStockProductViewModel
    {
        Name = p.Name,
        StockQuantity = p.Quantity,
        QuantitySold = _context.OrderDetails
            .Where(od => od.ProductId == p.Id)
            .Where(od => od.Order.Status == OrderStatus.Delivered)
            .Sum(od => (int?)od.Quantity) ?? 0
    })
    .ToListAsync();

            var productStats = new ProductStatisticsViewModel
            {
                TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
                OutOfStockProducts = await _context.Products.CountAsync(p => p.IsActive && p.Quantity <= 0),
                TopSellingProducts = await GetTopSellingProducts(10),
                LeastSellingProducts = await GetLeastSellingProducts(10),
                ProductsByCategoryDistribution = await GetProductsByCategoryDistribution(),
                ListOutOfStock = outOfStockList
            };

            return View(productStats);
        }

        // Flash Sale Statistics
        public async Task<IActionResult> FlashSaleStatistics()
        {
            var flashSaleStats = new FlashSaleStatisticsViewModel
            {
                TotalFlashSales = await _context.FlashSales.CountAsync(),
                ActiveFlashSales = await _context.FlashSales.CountAsync(f =>
                    f.IsActive && f.StartTime <= DateTime.Now && f.EndTime >= DateTime.Now),
                UpcomingFlashSales = await _context.FlashSales.CountAsync(f =>
                    f.IsActive && f.StartTime > DateTime.Now),
                FlashSalePerformance = await GetFlashSalePerformance()
            };

            return View(flashSaleStats);
        }

        // Revenue Statistics
        public async Task<IActionResult> RevenueStatistics(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate?.AddDays(1).AddMilliseconds(-1) ?? DateTime.Now;

            var revenueStats = new RevenueStatisticsViewModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = await GetRevenueInPeriod(start, end),
                DailyRevenue = await GetDailyRevenue(start, end),
                RevenueByPaymentMethod = await GetRevenueByPaymentMethod(start, end),
                RevenueByCategory = await GetRevenueByCategory(start, end)
            };

            return View(revenueStats);
        }

        public async Task<IActionResult> DailyOrderDetails(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1).AddMilliseconds(-1);

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= startOfDay && o.OrderDate <= endOfDay)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.FlashSale)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }


        // Customer Statistics
        public async Task<IActionResult> CustomerStatistics()
        {
            var customerStats = new CustomerStatisticsViewModel
            {
                TotalCustomers = await GetUniqueCustomerCount(),
                RepeatCustomers = await GetRepeatCustomerCount(),
                TopCustomers = await GetTopCustomers(10),
                NewCustomersThisMonth = await GetNewCustomersThisMonth()
            };

            return View(customerStats);
        }

        // Export Statistics to Excel/CSV

        private string GetPaymentMethodDisplay(string key)
        {
            return key switch
            {
                "1" => "Thanh toán khi nhận hàng",
                "2" => "Thanh toán Online",
                _ => key
            };
        }

        //public async Task<IActionResult> ExportSalesData(DateTime? startDate, DateTime? endDate, string format = "excel")
        //{
        //    var start = startDate ?? DateTime.Now.AddMonths(-1);
        //    var end = endDate?.AddDays(1).AddMilliseconds(-1) ?? DateTime.Now;

        //    var orders = await _context.Orders
        //        .Include(o => o.OrderDetails).ThenInclude(o => o.FlashSale)
        //        .Where(o => o.OrderDate.Date >= start.Date && o.OrderDate.Date <= end.Date)
        //        .OrderBy(o => o.OrderDate)
        //        .ToListAsync();

        //    var revenueStats = new RevenueStatisticsViewModel
        //    {
        //        StartDate = start,
        //        EndDate = end,
        //        TotalRevenue = await GetRevenueInPeriod(start, end),
        //        DailyRevenue = await GetDailyRevenue(start, end),
        //        RevenueByPaymentMethod = await GetRevenueByPaymentMethod(start, end),
        //        RevenueByCategory = await GetRevenueByCategory(start, end)
        //    };

        //    // Kiểm tra format và xuất dữ liệu
        //    if (format.ToLower() == "excel")
        //    {
        //        return ExportToExcel(orders, revenueStats, start, end);
        //    }
        //    else if (format.ToLower() == "pdf")
        //    {
        //        return ExportToPdf(orders, revenueStats, start, end);
        //    }

        //    return BadRequest("Chỉ hỗ trợ xuất Excel hoặc PDF trong phiên bản này.");
        //}
        public async Task<IActionResult> ExportSalesData(DateTime? startDate, DateTime? endDate, string format = "excel")
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate?.AddDays(1).AddMilliseconds(-1) ?? DateTime.Now;

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate.Date >= start.Date && o.OrderDate.Date <= end.Date)
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            // Lấy danh sách tất cả thời điểm đặt hàng trong khoảng thời gian
            var allOrderTimes = orders.Select(o => o.OrderDate).Distinct().ToList();

            // Lấy tất cả flash sale có khả năng hoạt động trong các thời điểm trong khoảng thời gian
            var flashSales = await _context.FlashSales
                .Include(fs => fs.FlashSaleProducts)
                .Where(fs => allOrderTimes.Any(orderDate => fs.StartTime <= orderDate && fs.EndTime >= orderDate))
                .ToListAsync();

            // Ánh xạ từng đơn hàng và từng sản phẩm trong đơn hàng với flash sale tương ứng
            foreach (var order in orders)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var matchingFlash = flashSales
                        .Where(fs => fs.StartTime <= order.OrderDate && fs.EndTime >= order.OrderDate)
                        .SelectMany(fs => fs.FlashSaleProducts, (fs, fsp) => new { fs, fsp })
                        .FirstOrDefault(x => x.fsp.ProductId == detail.ProductId);

                    if (matchingFlash != null)
                    {
                        detail.FlashSaleId = matchingFlash.fs.Id;
                        detail.FlashSale = matchingFlash.fs;
                    }
                }
            }

            var revenueStats = new RevenueStatisticsViewModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = await GetRevenueInPeriod(start, end),
                DailyRevenue = await GetDailyRevenue(start, end),
                RevenueByPaymentMethod = await GetRevenueByPaymentMethod(start, end),
                RevenueByCategory = await GetRevenueByCategory(start, end)
            };

            // Kiểm tra format và xuất dữ liệu
            if (format.ToLower() == "excel")
            {
                return ExportToExcel(orders, revenueStats, start, end);
            }
            else if (format.ToLower() == "pdf")
            {
                return ExportToPdf(orders, revenueStats, start, end);
            }

            return BadRequest("Chỉ hỗ trợ xuất Excel hoặc PDF trong phiên bản này.");
        }

        private IActionResult ExportToExcel(List<LHBooksWeb.Models.EF.Order> orders, RevenueStatisticsViewModel revenueStats, DateTime startDate, DateTime endDate)
        {
            var ordersByDate = orders.GroupBy(o => o.OrderDate.Date).OrderBy(g => g.Key);

            using (var workbook = new XLWorkbook())
            {
                var summarySheet = workbook.Worksheets.Add("Tổng Quan Doanh Thu");

                summarySheet.Cell(1, 1).Value = "Từ ngày:";
                summarySheet.Cell(1, 2).Value = revenueStats.StartDate.ToString("dd/MM/yyyy");

                summarySheet.Cell(2, 1).Value = "Đến ngày:";
                summarySheet.Cell(2, 2).Value = revenueStats.EndDate.ToString("dd/MM/yyyy");

                summarySheet.Cell(3, 1).Value = "Tổng doanh thu:";
                summarySheet.Cell(3, 2).Value = revenueStats.TotalRevenue;
                summarySheet.Cell(3, 2).Style.NumberFormat.Format = "#,##0₫";

                // Dòng trống
                int row = 5;
                summarySheet.Cell(row++, 1).Value = "Doanh thu theo phương thức thanh toán";
                summarySheet.Cell(row, 1).Value = "Phương thức";
                summarySheet.Cell(row, 2).Value = "Tổng doanh thu";
                row++;

                foreach (var item in revenueStats.RevenueByPaymentMethod)
                {
                    summarySheet.Cell(row, 1).Value = item.Key;
                    summarySheet.Cell(row, 2).Value = item.Value;
                    summarySheet.Cell(row, 1).Value = GetPaymentMethodDisplay(item.Key);
                    row++;
                }

                row++; // Dòng trống

                summarySheet.Cell(row++, 1).Value = "Doanh thu theo danh mục";
                summarySheet.Cell(row, 1).Value = "Danh mục";
                summarySheet.Cell(row, 2).Value = "Tổng doanh thu";
                row++;

                foreach (var item in revenueStats.RevenueByCategory)
                {
                    summarySheet.Cell(row, 1).Value = item.Key;
                    summarySheet.Cell(row, 2).Value = item.Value;
                    summarySheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0₫";
                    row++;
                }

                // Căn chỉnh cột
                summarySheet.Columns().AdjustToContents();

                // Tính hàng cuối cùng của vùng tổng quan
                int lastSummaryRow = row - 1;

                // Áp dụng viền cho toàn bộ vùng từ A1 đến B[lastSummaryRow]
                var borderRange = summarySheet.Range($"A1:B{lastSummaryRow}");
                borderRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                borderRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                borderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                borderRange.Style.Font.Bold = true;
                summarySheet.Range("A5:B5").Style.Font.Bold = true;
                summarySheet.Range("A5:B5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Sheet chi tiết đơn hàng
                foreach (var group in ordersByDate)
                {
                    var date = group.Key;
                    var worksheet = workbook.Worksheets.Add(date.ToString("yyyy-MM-dd"));

                    // Header
                    worksheet.Cell(1, 1).Value = "Mã đơn";
                    worksheet.Cell(1, 2).Value = "Khách hàng";
                    worksheet.Cell(1, 3).Value = "SĐT";
                    worksheet.Cell(1, 4).Value = "Địa chỉ";
                    worksheet.Cell(1, 5).Value = "Email";
                    worksheet.Cell(1, 6).Value = "Sản phẩm";
                    worksheet.Cell(1, 7).Value = "Số lượng";
                    worksheet.Cell(1, 8).Value = "Giá (vnđ)";
                    worksheet.Cell(1, 9).Value = "Thành tiền";
                    worksheet.Cell(1, 10).Value = "FlashSale"; // Thêm cột FlashSale
                    worksheet.Cell(1, 11).Value = "Phí vận chuyển";
                    worksheet.Cell(1, 12).Value = "Tổng tiền đơn hàng";
                    worksheet.Cell(1, 13).Value = "Trạng thái đơn hàng";

                    for (int col = 1; col <= 12; col++)
                    {
                        worksheet.Cell(1, col).Style.Font.Bold = true;
                        worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    int detailRow = 2;
                    var random = new Random();
                    var orderColorMap = new Dictionary<string, XLColor>();

                    foreach (var order in group)
                    {
                        // Gán màu nếu chưa có cho mã đơn hàng
                        if (!orderColorMap.ContainsKey(order.Code))
                        {
                            // Tạo màu ngẫu nhiên nhạt để dễ nhìn
                            var color = XLColor.FromArgb(
                                200 + random.Next(0, 56),
                                200 + random.Next(0, 56),
                                200 + random.Next(0, 56));
                            orderColorMap[order.Code] = color;
                        }

                        foreach (var detail in order.OrderDetails)
                        {
                            worksheet.Cell(detailRow, 1).Value = order.Code;
                            worksheet.Cell(detailRow, 2).Value = order.CustomerName;
                            worksheet.Cell(detailRow, 3).Value = order.Phone;
                            worksheet.Cell(detailRow, 4).Value = order.Address;
                            worksheet.Cell(detailRow, 5).Value = order.Email;
                            worksheet.Cell(detailRow, 6).Value = detail.ProductName ?? detail.Product?.Name;
                            worksheet.Cell(detailRow, 7).Value = detail.Quantity;
                            worksheet.Cell(detailRow, 8).Value = detail.Price;
                            worksheet.Cell(detailRow, 9).Value = detail.SubTotal;

                            // Thêm thông tin FlashSale
                            worksheet.Cell(detailRow, 10).Value = detail.FlashSale != null ? detail.FlashSale.Title : "Không";
                            worksheet.Cell(detailRow, 11).Value = order.ShippingFee != null ? order.ShippingFee: "0";
                            worksheet.Cell(detailRow, 12).Value = order.TotalAmount;
                            worksheet.Cell(detailRow, 13).Value = order.Status.GetDisplayName();

                            for (int col = 1; col <= 12; col++)
                            {
                                worksheet.Cell(detailRow, col).Style.Fill.BackgroundColor = orderColorMap[order.Code];
                            }

                            detailRow++;
                        }
                    }
                    worksheet.Columns().AdjustToContents();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"ThongKe_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx");
                }
            }
        }


        //private IActionResult ExportToPdf(List<LHBooksWeb.Models.EF.Order> orders, RevenueStatisticsViewModel revenueStats, DateTime startDate, DateTime endDate)
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        // Tạo đối tượng PdfWriter
        //        var writer = new PdfWriter(ms);
        //        var pdf = new PdfDocument(writer);
        //        var document = new Document(pdf);

        //        // Tiêu đề - Sử dụng Text để cài đặt kiểu chữ
        //        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        //        var title = new Paragraph("Danh sách đơn hàng")
        //            .SetFont(boldFont)
        //            .SetFontSize(16)
        //            .SetTextAlignment(TextAlignment.CENTER);

        //        document.Add(title);

        //        // Thêm thông tin tổng quan doanh thu
        //        document.Add(new Paragraph($"Từ ngày: {revenueStats.StartDate:dd/MM/yyyy}"));
        //        document.Add(new Paragraph($"Đến ngày: {revenueStats.EndDate:dd/MM/yyyy}"));
        //        document.Add(new Paragraph($"Tổng doanh thu: {revenueStats.TotalRevenue:C}"));

        //        // Tạo bảng
        //        var table = new Table(11);
        //        table.AddCell("Mã đơn");
        //        table.AddCell("Khách hàng");
        //        table.AddCell("SĐT");
        //        table.AddCell("Địa chỉ");
        //        table.AddCell("Email");
        //        table.AddCell("Sản phẩm");
        //        table.AddCell("Số lượng");
        //        table.AddCell("Giá");
        //        table.AddCell("Thành tiền");
        //        table.AddCell("Tổng tiền đơn hàng");
        //        table.AddCell("Trạng thái đơn hàng");

        //        // Thêm các đơn hàng vào bảng
        //        foreach (var order in orders)
        //        {
        //            foreach (var detail in order.OrderDetails)
        //            {
        //                table.AddCell(order.Code);
        //                table.AddCell(order.CustomerName);
        //                table.AddCell(order.Phone);
        //                table.AddCell(order.Address);
        //                table.AddCell(order.Email);
        //                table.AddCell(detail.ProductName ?? detail.Product?.Name);
        //                table.AddCell(detail.Quantity.ToString());
        //                table.AddCell(detail.Price.ToString("#,##0₫"));
        //                table.AddCell(detail.SubTotal.ToString("#,##0₫"));
        //                table.AddCell(order.TotalAmount.ToString("#,##0₫"));
        //                table.AddCell(order.Status.GetDisplayName());
        //            }
        //        }

        //        document.Add(table);

        //        // Lưu file PDF và trả về
        //        document.Close();
        //        return File(ms.ToArray(), "application/pdf", $"SalesData_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf");
        //    }
        //}
        private IActionResult ExportToPdf(List<LHBooksWeb.Models.EF.Order> orders, RevenueStatisticsViewModel revenueStats, DateTime startDate, DateTime endDate)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);

                // Khổ giấy ngang
                var pageSize = PageSize.A4.Rotate();
                var document = new Document(pdf, pageSize);
                document.SetMargins(20, 20, 20, 20);

                // Đường dẫn tới file font Roboto đã thêm
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/font/Roboto-Regular.ttf");

                // Tạo font với encoding hỗ trợ Unicode tiếng Việt
                var vietnameseFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, EmbeddingStrategy.PREFER_EMBEDDED);


                // Tiêu đề
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var title = new Paragraph("Danh sách đơn hàng")
                    .SetFont(boldFont)
                    .SetFontSize(16).SetFont(vietnameseFont)
                    .SetTextAlignment(TextAlignment.CENTER);

                document.Add(title);

                // Thông tin doanh thu
                document.Add(new Paragraph($"Từ ngày: {revenueStats.StartDate:dd/MM/yyyy}").SetFont(vietnameseFont));
                document.Add(new Paragraph($"Đến ngày: {revenueStats.EndDate:dd/MM/yyyy}").SetFont(vietnameseFont));
                document.Add(new Paragraph($"Tổng doanh thu: {revenueStats.TotalRevenue:C}").SetFont(vietnameseFont));

                // Tạo bảng
                var table = new Table(11, true); // 11 cột

                string[] headers = {
            "Mã đơn", "Khách hàng", "SĐT", "Địa chỉ", "Email",
            "Sản phẩm", "Số lượng", "Giá", "Thành tiền", "Tổng tiền đơn hàng", "Trạng thái đơn hàng"
        };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(header).SetFont(vietnameseFont)));
                }

                foreach (var order in orders)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        table.AddCell(new Paragraph(order.Code).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(order.CustomerName).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(order.Phone).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(order.Address).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(order.Email).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(detail.ProductName ?? detail.Product?.Name).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(detail.Quantity.ToString()).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(detail.Price.ToString("#,##0₫")).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(detail.SubTotal.ToString("#,##0₫")).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(order.TotalAmount.ToString("#,##0₫")).SetFont(vietnameseFont));
                        table.AddCell(new Paragraph(order.Status.GetDisplayName()).SetFont(vietnameseFont));
                    }
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", $"ThongKe_Pdf_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf");
            }
        }





        #region Helper Methods

        private async Task<decimal> GetTotalSales()
        {
            return await _context.Orders
                .Where(o => ((o.TypePayment == 2 &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.AwaitingPayment &&
                        o.Status != OrderStatus.PaymentFailed)
                       ||
                       (o.TypePayment == 1 && o.Status == OrderStatus.Delivered)))
                .SumAsync(o => o.TotalAmount); // Tính tổng doanh thu của TotalAmount
        }


        private async Task<int> GetTotalOrders()
        {
            return await _context.Orders.CountAsync();
        }

        private async Task<int> GetTotalProducts()
        {
            return await _context.Products.CountAsync();
        }

        private async Task<int> GetNewOrdersCount()
        {
            return await _context.Orders
                .CountAsync(o =>
                    (o.TypePayment == 2 && o.Status == OrderStatus.Paid) ||
                    (o.TypePayment == 1 && o.Status == OrderStatus.Pending));
        }



        private async Task<List<TopSellingProductViewModel>> GetTopSellingProducts(int count)
        {

            var currentTime = DateTime.Now;

            // First, get the active flash sales
            var activeFlashSaleProducts = await _context.FlashSaleProducts
                .Where(fsp => fsp.FlashSale.IsActive &&
                             fsp.FlashSale.StartTime <= currentTime &&
                             fsp.FlashSale.EndTime >= currentTime)
                .ToListAsync();

            // Create a dictionary for quick lookup of flash sale prices
            var flashSalePrices = activeFlashSaleProducts.ToDictionary(
                fsp => fsp.ProductId,
                fsp => fsp.FlashPrice
            );
            // Lấy danh sách sản phẩm đã bán, sắp xếp theo số lượng bán giảm dần
            var result = await _context.OrderDetails
    .Where(od =>
        (od.Order.TypePayment == 2 &&
         od.Order.Status != OrderStatus.Cancelled &&
         od.Order.Status != OrderStatus.AwaitingPayment &&
         od.Order.Status != OrderStatus.PaymentFailed)
        ||
        (od.Order.TypePayment == 1 &&
         od.Order.Status == OrderStatus.Delivered)
    )
    .GroupBy(od => od.ProductId)
    .Select(g => new
    {
        ProductId = g.Key,
        QuantitySold = g.Sum(od => od.Quantity),
        Revenue = g.Sum(od => od.Price * od.Quantity)
    })
    .OrderByDescending(x => x.QuantitySold)
    .Take(count)
    .ToListAsync();


            // Lấy thông tin chi tiết sản phẩm
            var productIds = result.Select(r => r.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();


            // Ghép thông tin lại
            var topSellingProducts = result.Select(r =>
            {
                var product = products.FirstOrDefault(p => p.Id == r.ProductId);

                decimal currentPrice;

                // Check if product is in an active flash sale
                if (flashSalePrices.ContainsKey(r.ProductId))
                {
                    currentPrice = flashSalePrices[r.ProductId];
                }
                // Check if product is on sale
                else if (product != null && product.IsSale)
                {
                    currentPrice = product.PriceSale;
                }
                // Regular price
                else if (product != null)
                {
                    currentPrice = product.Price;
                }
                else
                {
                    currentPrice = 0; // Product not found (unlikely case)
                }
                return new TopSellingProductViewModel
                {
                    ProductId = r.ProductId,
                    ProductCode = product?.ProductCode ?? "N/A",
                    ProductName = product?.Name ?? "N/A",
                    QuantitySold = r.QuantitySold,
                    Revenue = r.Revenue,
                    StockQuantity = product?.Quantity ?? 0,
                    CurrentPrice = currentPrice,                  
                };
            }).ToList();

            return topSellingProducts;
        }


        private async Task<List<TopSellingProductViewModel>> GetLeastSellingProducts(int count)
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .GroupJoin(_context.OrderDetails,
                    p => p.Id,
                    od => od.ProductId,
                    (p, orderDetails) => new
                    {
                        Product = p,
                        OrderDetails = orderDetails
                    })
                .Select(x => new TopSellingProductViewModel
                {
                    ProductId = x.Product.Id,
                    ProductCode = x.Product.ProductCode,
                    ProductName = x.Product.Name,
                    QuantitySold = x.OrderDetails
                        .Where(od =>
                            (od.Order.TypePayment == 2 &&
                             od.Order.Status != OrderStatus.Cancelled &&
                             od.Order.Status != OrderStatus.AwaitingPayment &&
                             od.Order.Status != OrderStatus.PaymentFailed)
                            ||
                            (od.Order.TypePayment == 1 &&
                             od.Order.Status == OrderStatus.Delivered)
                        )
                        .Sum(od => od.Quantity),
                    Revenue = x.OrderDetails
                        .Where(od =>
                            (od.Order.TypePayment == 2 &&
                             od.Order.Status != OrderStatus.Cancelled &&
                             od.Order.Status != OrderStatus.AwaitingPayment &&
                             od.Order.Status != OrderStatus.PaymentFailed)
                            ||
                            (od.Order.TypePayment == 1 &&
                             od.Order.Status == OrderStatus.Delivered)
                        )
                        .Sum(od => od.Price * od.Quantity),
                    StockQuantity = x.Product.Quantity
                })
                .OrderBy(x => x.QuantitySold)
                .Take(count)
                .ToListAsync();
        }


        private async Task<Dictionary<string, int>> GetMonthlySalesData()
        {
            var startDate = DateTime.Now.AddMonths(-11);
            var endDate = DateTime.Now;

            // First, get data from database with minimal transformation
            var monthlyData = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate &&
                          ((o.TypePayment == 2 &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.AwaitingPayment &&
                        o.Status != OrderStatus.PaymentFailed)
                       ||
                       (o.TypePayment == 1 && o.Status == OrderStatus.Delivered)))
                .GroupBy(o => new { Month = o.OrderDate.Month, Year = o.OrderDate.Year })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // Then perform the string formatting in memory
            var monthlySales = new Dictionary<string, int>();
            foreach (var item in monthlyData)
            {
                string monthYear = $"{item.Year}-{item.Month:D2}";
                monthlySales.Add(monthYear, item.Total);
            }

            return monthlySales;
        }

        private async Task<Dictionary<string, int>> GetOrderStatusDistribution()
        {
            var distribution = new Dictionary<string, int>();

            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                // Get count from database
                var count = await _context.Orders.CountAsync(o => o.Status == status);

                // Get the Display attribute value (Vietnamese name)
                var displayName = GetDisplayName(status);

                distribution.Add(displayName, count);
            }

            return distribution;
        }

        private string GetDisplayName(OrderStatus status)
        {
            // Get the Display attribute for the enum value
            var fieldInfo = status.GetType().GetField(status.ToString());
            var displayAttribute = (DisplayAttribute)fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

            // Return the Display Name if it exists, otherwise return the enum name
            return displayAttribute?.Name ?? status.ToString();
        }

        //private async Task<List<DailyOrdersViewModel>> GetOrdersByDate(int days)
        //{
        //    var startDate = DateTime.Now.AddDays(-days);
        //    var endDate = DateTime.Now;

        //    return await _context.Orders
        //        .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate  && o.Status != OrderStatus.Cancelled)
        //        .GroupBy(o => o.OrderDate.Date)
        //        .Select(g => new DailyOrdersViewModel
        //        {
        //            Date = g.Key,
        //            OrderCount = g.Count(),
        //            Revenue = g.Sum(o => o.TotalAmount)
        //        })
        //        .OrderBy(x => x.Date)
        //        .ToListAsync();
        //}
        private async Task<List<DailyOrdersViewModel>> GetOrdersByDate(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate &&
                           (
                               (o.TypePayment == 2 &&
                                o.Status != OrderStatus.Cancelled &&
                                o.Status != OrderStatus.AwaitingPayment &&
                                o.Status != OrderStatus.PaymentFailed)
                               ||
                               (o.TypePayment == 1 && o.Status == OrderStatus.Delivered)
                           ))
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyOrdersViewModel
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }



        private async Task<Dictionary<string, int>> GetProductsByCategoryDistribution()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .GroupBy(p => p.ProductCategory.Title)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }


        //private async Task<List<FlashSalePerformanceViewModel>> GetFlashSalePerformance()
        //{
        //    // Lấy danh sách tất cả các flash sale đã kết thúc
        //    var flashSales = await _context.FlashSales
        //        .Where(f => f.EndTime < DateTime.Now)
        //        .Include(f => f.FlashSaleProducts)
        //            .ThenInclude(fp => fp.Product)
        //                .ThenInclude(p => p.OrderDetails)
        //                    .ThenInclude(od => od.Order)
        //        .ToListAsync();

        //    // Thực hiện tính toán trong memory thay vì SQL
        //    var result = flashSales.Select(f => new FlashSalePerformanceViewModel
        //    {
        //        FlashSaleId = f.Id,
        //        Title = f.Title,
        //        StartTime = f.StartTime,
        //        EndTime = f.EndTime,
        //        TotalProducts = f.FlashSaleProducts.Count,
        //        EstimatedRevenue = f.FlashSaleProducts.Sum(fp =>
        //            fp.Product.OrderDetails
        //                .Where(od => od.Order.OrderDate >= f.StartTime && od.Order.OrderDate <= f.EndTime)
        //                .Sum(od => od.Quantity * od.Price)
        //        )
        //    }).ToList();

        //    return result;
        //}

        private async Task<List<FlashSalePerformanceViewModel>> GetFlashSalePerformance()
        {
            var now = DateTime.Now;
            var flashSales = await _context.FlashSales
                .Include(f => f.FlashSaleProducts)
                    .ThenInclude(fp => fp.Product)
                        .ThenInclude(p => p.OrderDetails)
                            .ThenInclude(od => od.Order)
                .ToListAsync();

            var result = flashSales.Select(f => new FlashSalePerformanceViewModel
            {
                FlashSaleId = f.Id,
                Title = f.Title,
                StartTime = f.StartTime,
                EndTime = f.EndTime,
                IsActive = f.StartTime <= now && f.EndTime >= now,
                TotalProducts = f.FlashSaleProducts.Count,
                EstimatedRevenue = f.FlashSaleProducts.Sum(fp =>
    fp.Product.OrderDetails
        .Where(od =>
            od.Order.OrderDate >= f.StartTime &&
            od.Order.OrderDate <= f.EndTime &&
            (
                (od.Order.TypePayment == 2 &&
                 od.Order.Status != OrderStatus.Cancelled &&
                 od.Order.Status != OrderStatus.AwaitingPayment &&
                 od.Order.Status != OrderStatus.PaymentFailed)
                ||
                (od.Order.TypePayment == 1 &&
                 od.Order.Status == OrderStatus.Delivered)
            )
        )
        .Sum(od => od.Quantity * od.Price)
)

            }).ToList();

            return result;
        }


        private async Task<decimal> GetRevenueInPeriod(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate
                            && ((o.TypePayment == 2 &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.AwaitingPayment &&
                        o.Status != OrderStatus.PaymentFailed)
                       ||
                       (o.TypePayment == 1 && o.Status == OrderStatus.Delivered)))
                .SumAsync(o => o.TotalAmount); // Tính tổng của TotalAmount
        }



        private async Task<List<DailyRevenueViewModel>> GetDailyRevenue(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate &&
                           ((o.TypePayment == 2 &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.AwaitingPayment &&
                        o.Status != OrderStatus.PaymentFailed)
                       ||
                       (o.TypePayment == 1 && o.Status == OrderStatus.Delivered)))
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyRevenueViewModel
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        private async Task<Dictionary<string, decimal>> GetRevenueByPaymentMethod(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate &&
                           ((o.TypePayment == 2 &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.AwaitingPayment &&
                        o.Status != OrderStatus.PaymentFailed)
                       ||
                       (o.TypePayment == 1 && o.Status == OrderStatus.Delivered)))
                .GroupBy(o => o.TypePayment)
                .Select(g => new
                {
                    PaymentMethod = g.Key.ToString(), // This would need a mapping to readable names
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .ToDictionaryAsync(x => x.PaymentMethod, x => x.Revenue);
        }

        private async Task<Dictionary<string, decimal>> GetRevenueByCategory(DateTime startDate, DateTime endDate)
        {
            return await _context.OrderDetails
                .Where(od => od.Order.OrderDate >= startDate &&
                             od.Order.OrderDate <= endDate &&
                            (
                                (od.Order.TypePayment == 2 &&
                                 od.Order.Status != OrderStatus.Cancelled &&
                                 od.Order.Status != OrderStatus.AwaitingPayment &&
                                 od.Order.Status != OrderStatus.PaymentFailed)
                                ||
                                (od.Order.TypePayment == 1 && od.Order.Status == OrderStatus.Delivered)
                            ))
                .GroupBy(od => od.Product.ProductCategory.Title)
                .Select(g => new
                {
                    Category = g.Key,
                    Revenue = g.Sum(od => od.Price * od.Quantity)
                })
                .ToDictionaryAsync(x => x.Category, x => x.Revenue);
        }


        private async Task<int> GetUniqueCustomerCount()
        {
            // This assumes customers are identified by a combination of email/phone
            return await _context.Orders
                .Select(o => new { o.Email, o.Phone })
                .Distinct()
                .CountAsync();
        }

        private async Task<int> GetRepeatCustomerCount()
        {
            // Customers who have placed more than one order
            return await _context.Orders
                .GroupBy(o => new { o.Email, o.Phone })
                .Where(g => g.Count() > 1)
                .CountAsync();
        }

        private async Task<List<TopCustomerViewModel>> GetTopCustomers(int count)
        {
            return await _context.Orders
                .Where(o => o.TypePayment == 2 &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.AwaitingPayment &&
                        o.Status != OrderStatus.PaymentFailed
                       ||
                       o.TypePayment == 1 && o.Status == OrderStatus.Delivered)
                .GroupBy(o => new { o.CustomerName, o.Email, o.Phone })
                .Select(g => new TopCustomerViewModel
                {
                    CustomerName = g.Key.CustomerName,
                    Email = g.Key.Email,
                    Phone = g.Key.Phone,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(count)
                .ToListAsync();
        }

        private async Task<int> GetNewCustomersThisMonth()
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Get distinct customers from this month
            return await _context.Orders
                .Where(o => o.OrderDate >= firstDayOfMonth)
                .Select(o => new { o.Email, o.Phone })
                .Distinct()
                .CountAsync();
        }
        private async Task<int> GetActiveFlashSaleCount()
        {
            var currentDate = DateTime.Now;
            var count = await _context.FlashSales
                .CountAsync(fs => fs.IsActive && fs.StartTime <= currentDate && fs.EndTime >= currentDate);

            return count;
        }

        #endregion
    }

}
