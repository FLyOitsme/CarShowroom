using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Services;
using DataLayer.Entities;

namespace CarShowroom.ViewModels
{
    public partial class SalesListPageViewModel : ObservableObject
    {
        private readonly SaleService _saleService;

        [ObservableProperty]
        private List<Sale> _sales = new();

        [ObservableProperty]
        private Sale? _selectedSale;

        public SalesListPageViewModel(SaleService saleService)
        {
            _saleService = saleService;
        }

        [RelayCommand]
        private async Task LoadSalesAsync()
        {
            try
            {
                Sales = await _saleService.GetAllSalesAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить продажи: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SaleSelectedAsync(Sale? sale)
        {
            if (sale != null)
            {
                await Shell.Current.DisplayAlert("Продажа",
                    $"Автомобиль: {sale.Car?.Model?.Brand?.Name} {sale.Car?.Model?.Name}\n" +
                    $"Менеджер: {sale.Manager?.Name} {sale.Manager?.Surname}\n" +
                    $"Дата: {sale.Date}\n" +
                    $"Цена: {sale.Cost:N0} ₽",
                    "OK");
                SelectedSale = null;
            }
        }

        [RelayCommand]
        private async Task AddSaleAsync()
        {
            await Shell.Current.GoToAsync("CreateSalePage");
        }
    }
}

