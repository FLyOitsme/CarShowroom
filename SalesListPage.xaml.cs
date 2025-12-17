using CarShowroom.Services;
using DataLayer.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CarShowroom
{
    public partial class SalesListPage : ContentPage
    {
        private SaleService? _saleService;

        public SalesListPage()
        {
            InitializeComponent();
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            // Используем Dispatcher для получения сервисов после полной загрузки
            Dispatcher.DispatchAsync(async () =>
            {
                await InitializeServicesAsync();
            });
        }

        private async Task InitializeServicesAsync()
        {
            // Ждем, пока Handler полностью инициализирован
            var maxAttempts = 10;
            var attempt = 0;
            
            while (_saleService == null && attempt < maxAttempts)
            {
                if (Handler?.MauiContext?.Services != null)
                {
                    _saleService = Handler.MauiContext.Services.GetService<SaleService>();
                    if (_saleService != null)
                    {
                        await LoadSalesAsync();
                        return;
                    }
                }
                
                await Task.Delay(50);
                attempt++;
            }
            
            if (_saleService == null)
            {
                await DisplayAlert("Ошибка", "SaleService не найден", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Если сервис еще не получен, пытаемся получить
            if (_saleService == null)
            {
                await InitializeServicesAsync();
            }
            else
            {
                await LoadSalesAsync();
            }
        }

        private async Task LoadSalesAsync()
        {
            if (_saleService == null)
            {
                await DisplayAlert("Ошибка", "SaleService не инициализирован", "OK");
                return;
            }

            try
            {
                var sales = await _saleService.GetAllSalesAsync();
                SalesCollectionView.ItemsSource = sales;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить продажи: {ex.Message}", "OK");
            }
        }

        private async void OnSaleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Sale selectedSale)
            {
                // Можно открыть детали продажи
                await DisplayAlert("Продажа", 
                    $"Автомобиль: {selectedSale.Car?.Model?.Brand?.Name} {selectedSale.Car?.Model?.Name}\n" +
                    $"Менеджер: {selectedSale.Manager?.Name} {selectedSale.Manager?.Surname}\n" +
                    $"Дата: {selectedSale.Date}\n" +
                    $"Цена: {selectedSale.Cost:N0} ₽", 
                    "OK");
                SalesCollectionView.SelectedItem = null;
            }
        }

        private async void OnAddSaleClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateSalePage());
        }
    }
}

