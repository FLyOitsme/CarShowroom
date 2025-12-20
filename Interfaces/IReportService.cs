namespace CarShowroom.Interfaces
{
    public interface IReportService
    {
        Task<int> GetTotalSalesCountAsync();
        Task<int> GetTotalSalesCountByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<int> GetAvailableCarsCountAsync();
        Task<int> GetSoldCarsCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetAverageSalePriceAsync();
        Task<int> GetActiveDiscountsCountAsync();
        Task<Dictionary<string, int>> GetSalesByBrandAsync();
        Task<Dictionary<string, int>> GetSalesByManagerAsync();
        Task<List<MonthlyStatistic>> GetMonthlyStatisticsAsync(int year);
    }

    public class MonthlyStatistic
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }
}

