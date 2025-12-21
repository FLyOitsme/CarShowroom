using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using Microsoft.Maui.ApplicationModel;

namespace CarShowroom.ViewModels
{
    public partial class ReportsPageViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

        [ObservableProperty]
        private int _totalSales = 0;

        [ObservableProperty]
        private int _availableCars = 0;

        [ObservableProperty]
        private int _soldCars = 0;

        [ObservableProperty]
        private decimal _totalRevenue = 0;

        [ObservableProperty]
        private decimal _averageSalePrice = 0;

        [ObservableProperty]
        private int _activeDiscounts = 0;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private DateTime _periodStartDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _periodEndDate = DateTime.Now;

        [ObservableProperty]
        private int _periodSalesCount = 0;

        [ObservableProperty]
        private decimal _periodRevenue = 0;

        [ObservableProperty]
        private Dictionary<string, int> _salesByBrand = new();

        [ObservableProperty]
        private Dictionary<string, int> _salesByManager = new();

        [ObservableProperty]
        private List<BrandSalesItem> _salesByBrandItems = new();

        [ObservableProperty]
        private List<ManagerSalesItem> _salesByManagerItems = new();

        [ObservableProperty]
        private List<MonthlyStatistic> _monthlyStatistics = new();

        [ObservableProperty]
        private int _selectedYear = DateTime.Now.Year;

        public List<int> YearOptions { get; } = new();

        public ReportsPageViewModel(IReportService reportService)
        {
            _reportService = reportService;
            
            var currentYear = DateTime.Now.Year;
            for (int i = 0; i < 6; i++)
            {
                YearOptions.Add(currentYear - i);
            }
        }

        [RelayCommand]
        private async Task LoadReportsAsync()
        {
            if (IsLoading) return; // Предотвращаем параллельные вызовы
            
            IsLoading = true;
            try
            {
                TotalSales = await _reportService.GetTotalSalesCountAsync().ConfigureAwait(false);
                AvailableCars = await _reportService.GetAvailableCarsCountAsync().ConfigureAwait(false);
                SoldCars = await _reportService.GetSoldCarsCountAsync().ConfigureAwait(false);
                TotalRevenue = await _reportService.GetTotalRevenueAsync().ConfigureAwait(false);
                AverageSalePrice = await _reportService.GetAverageSalePriceAsync().ConfigureAwait(false);
                ActiveDiscounts = await _reportService.GetActiveDiscountsCountAsync().ConfigureAwait(false);
                
                PeriodSalesCount = await _reportService.GetTotalSalesCountByPeriodAsync(PeriodStartDate, PeriodEndDate).ConfigureAwait(false);
                PeriodRevenue = await _reportService.GetRevenueByPeriodAsync(PeriodStartDate, PeriodEndDate).ConfigureAwait(false);
                
                SalesByBrand = await _reportService.GetSalesByBrandAsync().ConfigureAwait(false);
                SalesByManager = await _reportService.GetSalesByManagerAsync().ConfigureAwait(false);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    SalesByBrandItems = SalesByBrand.Select(kvp => new BrandSalesItem 
                    { 
                        BrandName = kvp.Key, 
                        SalesCount = kvp.Value 
                    }).OrderByDescending(x => x.SalesCount).ToList();
                    
                    SalesByManagerItems = SalesByManager.Select(kvp => new ManagerSalesItem 
                    { 
                        ManagerName = kvp.Key, 
                        SalesCount = kvp.Value 
                    }).OrderByDescending(x => x.SalesCount).ToList();
                });
                
                MonthlyStatistics = await _reportService.GetMonthlyStatisticsAsync(SelectedYear).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить отчеты: {ex.Message}", "OK");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshPeriodStatisticsAsync()
        {
            if (IsLoading) return; // Предотвращаем параллельные вызовы
            
            try
            {
                PeriodSalesCount = await _reportService.GetTotalSalesCountByPeriodAsync(PeriodStartDate, PeriodEndDate).ConfigureAwait(false);
                PeriodRevenue = await _reportService.GetRevenueByPeriodAsync(PeriodStartDate, PeriodEndDate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось обновить статистику: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task RefreshYearStatisticsAsync()
        {
            if (IsLoading) return; // Предотвращаем параллельные вызовы
            
            try
            {
                MonthlyStatistics = await _reportService.GetMonthlyStatisticsAsync(SelectedYear).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить статистику: {ex.Message}", "OK");
            }
        }

        partial void OnPeriodStartDateChanged(DateTime value)
        {
            if (IsLoading) return; // Не обновляем, если идет загрузка
            _ = Task.Run(async () =>
            {
                await Task.Delay(300); // Небольшая задержка для дебаунса
                if (!IsLoading)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        RefreshPeriodStatisticsCommand.ExecuteAsync(null);
                    });
                }
            });
        }

        partial void OnPeriodEndDateChanged(DateTime value)
        {
            if (IsLoading) return; // Не обновляем, если идет загрузка
            _ = Task.Run(async () =>
            {
                await Task.Delay(300); // Небольшая задержка для дебаунса
                if (!IsLoading)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        RefreshPeriodStatisticsCommand.ExecuteAsync(null);
                    });
                }
            });
        }

        partial void OnSelectedYearChanged(int value)
        {
            if (IsLoading) return; // Не обновляем, если идет загрузка
            _ = Task.Run(async () =>
            {
                await Task.Delay(300); // Небольшая задержка для дебаунса
                if (!IsLoading)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        RefreshYearStatisticsCommand.ExecuteAsync(null);
                    });
                }
            });
        }
    }

    public class BrandSalesItem
    {
        public string BrandName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
    }

    public class ManagerSalesItem
    {
        public string ManagerName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
    }
}

