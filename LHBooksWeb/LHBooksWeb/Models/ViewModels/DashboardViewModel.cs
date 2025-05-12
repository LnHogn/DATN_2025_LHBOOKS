namespace LHBooksWeb.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int NewOrders { get; set; }
        public List<TopSellingProductViewModel> TopSellingProducts { get; set; }
        public Dictionary<string, int> MonthlySalesData { get; set; }
        public Dictionary<string, int> OrderStatusDistribution { get; set; }
        public int ActiveFlashSaleCount { get; set; }
        public int TotalProducts { get; set; }
    }

    // Order statistics view model
    public class OrderStatisticsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmeOrder {  get; set; }
        public int ShippingOrders { get; set; }
        public int PaymentFailed {  get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyOrdersViewModel> OrdersByDate { get; set; }
    }

    // Product statistics view model
    public class ProductStatisticsViewModel
    {
        public int TotalProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public List<TopSellingProductViewModel> TopSellingProducts { get; set; }
        public List<TopSellingProductViewModel> LeastSellingProducts { get; set; }
        public Dictionary<string, int> ProductsByCategoryDistribution { get; set; }

        public List<OutOfStockProductViewModel> ListOutOfStock { get; set; }
    }

    public class OutOfStockProductViewModel
    {
        public string Name { get; set; }
        public int StockQuantity { get; set; }
        public int QuantitySold { get; set; }
    }

    // Flash sale statistics view model
    public class FlashSaleStatisticsViewModel
    {
        public int TotalFlashSales { get; set; }
        public int ActiveFlashSales { get; set; }
        public int UpcomingFlashSales { get; set; }
        public List<FlashSalePerformanceViewModel> FlashSalePerformance { get; set; }
    }

    // Revenue statistics view model
    public class RevenueStatisticsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DailyRevenueViewModel> DailyRevenue { get; set; }
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; }
        public Dictionary<string, decimal> RevenueByCategory { get; set; }
    }

    // Customer statistics view model
    public class CustomerStatisticsViewModel
    {
        public int TotalCustomers { get; set; }
        public int RepeatCustomers { get; set; }
        public List<TopCustomerViewModel> TopCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
    }

    // Supporting view models
    public class TopSellingProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }   
        public string ProductName { get; set; }     
        public int QuantitySold { get; set; }     
        public decimal Revenue { get; set; }       
        public int StockQuantity { get; set; }
        public decimal CurrentPrice { get; set; }
    }


    public class DailyOrdersViewModel
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class FlashSalePerformanceViewModel
    {
        public int FlashSaleId { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalProducts { get; set; }
        public decimal EstimatedRevenue { get; set; }
         public bool IsActive { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopCustomerViewModel
    {
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

}
