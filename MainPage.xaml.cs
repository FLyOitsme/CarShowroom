using CarShowroom.Services;
using DataLayer.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CarShowroom
{
    public partial class MainPage : ContentPage
    {
        private CarService? _carService;
        private List<Car> _allCars = new();

        public MainPage()
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
            
            while (_carService == null && attempt < maxAttempts)
            {
                if (Handler?.MauiContext?.Services != null)
                {
                    _carService = Handler.MauiContext.Services.GetService<CarService>();
                    if (_carService != null)
                    {
                        await LoadCarsAsync();
                        return;
                    }
                }
                
                await Task.Delay(50);
                attempt++;
            }
            
            if (_carService == null)
            {
                await DisplayAlert("Ошибка", "CarService не найден", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Если сервис еще не получен, пытаемся получить
            if (_carService == null)
            {
                await InitializeServicesAsync();
            }
            else
            {
                await LoadCarsAsync();
            }
        }

        private async Task LoadCarsAsync()
        {
            if (_carService == null)
            {
                await DisplayAlert("Ошибка", "CarService не инициализирован", "OK");
                return;
            }

            try
            {
                _allCars = await _carService.GetAllCarsAsync();
                CarsCollectionView.ItemsSource = _allCars;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить автомобили: {ex.Message}", "OK");
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_carService == null) return;

            var searchText = e.NewTextValue?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                CarsCollectionView.ItemsSource = _allCars;
            }
            else
            {
                try
                {
                    var filteredCars = await _carService.SearchCarsAsync(searchText);
                    CarsCollectionView.ItemsSource = filteredCars;
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Ошибка", $"Ошибка поиска: {ex.Message}", "OK");
                }
            }
        }

        private async void OnCarSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Car selectedCar)
            {
                await Navigation.PushAsync(new CarDetailsPage((int)selectedCar.Id));
                CarsCollectionView.SelectedItem = null;
            }
        }

        private async void OnCarTapped(object sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is Car car)
            {
                await Navigation.PushAsync(new CarDetailsPage((int)car.Id));
            }
        }

        private async void OnAddCarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditCarPage());
        }
    }
}
