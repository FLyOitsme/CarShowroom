using CarShowroom.Services;
using DataLayer.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CarShowroom
{
    public partial class CarDetailsPage : ContentPage
    {
        private CarService? _carService;
        private readonly long _carId;
        private Car? _car;

        public CarDetailsPage(long carId)
        {
            InitializeComponent();
            _carId = carId;
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
                        await LoadCarDetailsAsync();
                        return;
                    }
                }
                
                await Task.Delay(50);
                attempt++;
            }
            
            if (_carService == null)
            {
                await DisplayAlert("Ошибка", "CarService не найден", "OK");
                await Navigation.PopAsync();
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
                await LoadCarDetailsAsync();
            }
        }

        private async Task LoadCarDetailsAsync()
        {
            if (_carService == null)
            {
                await DisplayAlert("Ошибка", "CarService не инициализирован", "OK");
                return;
            }

            try
            {
                _car = await _carService.GetCarByIdAsync(_carId);
                if (_car != null)
                {
                    var brandName = _car.Model?.Brand?.Name ?? "Неизвестно";
                    var modelName = _car.Model?.Name ?? "Неизвестно";
                    CarNameLabel.Text = $"{brandName} {modelName} ({_car.Year})";
                    PriceLabel.Text = $"{_car.Cost:N0} ₽";
                    BrandLabel.Text = brandName;
                    ModelLabel.Text = modelName;
                    YearLabel.Text = _car.Year?.ToString() ?? "—";
                    ColorLabel.Text = _car.Color ?? "—";
                    EngineLabel.Text = _car.EngType?.Name ?? "—";
                    EngineVolumeLabel.Text = _car.EngVol.HasValue ? $"{_car.EngVol:F1} л" : "—";
                    MileageLabel.Text = _car.Mileage.HasValue ? $"{_car.Mileage:N0} км" : "—";
                    TransmissionLabel.Text = _car.Transmission?.Name ?? "—";
                    DescriptionLabel.Text = $"Тип: {_car.Type?.Name ?? "—"}\n" +
                                          $"Привод: {_car.Wd?.Name ?? "—"}\n" +
                                          $"Мощность: {(_car.Power.HasValue ? $"{_car.Power} л.с." : "—")}\n" +
                                          $"Состояние: {_car.Condition?.Name ?? "—"}";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditCarPage((int)_carId));
        }

        private async void OnCreateContractClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateSalePage(_carId));
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Подтверждение",
                "Вы уверены, что хотите удалить этот автомобиль?",
                "Да",
                "Нет");

            if (answer)
            {
                if (_carService == null)
                {
                    await DisplayAlert("Ошибка", "CarService не инициализирован", "OK");
                    return;
                }

                try
                {
                    await _carService.DeleteCarAsync(_carId);
                    await Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Ошибка", $"Не удалось удалить автомобиль: {ex.Message}", "OK");
                }
            }
        }
    }
}
