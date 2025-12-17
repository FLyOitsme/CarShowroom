using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Services;
using DataLayer.Entities;

namespace CarShowroom.ViewModels
{
    public partial class CarDetailsPageViewModel : ObservableObject
    {
        private readonly CarService _carService;

        [ObservableProperty]
        private Car? _car;

        [ObservableProperty]
        private string _carName = string.Empty;

        [ObservableProperty]
        private string _price = string.Empty;

        [ObservableProperty]
        private string _brand = string.Empty;

        [ObservableProperty]
        private string _model = string.Empty;

        [ObservableProperty]
        private string _year = string.Empty;

        [ObservableProperty]
        private string _color = string.Empty;

        [ObservableProperty]
        private string _engine = string.Empty;

        [ObservableProperty]
        private string _engineVolume = string.Empty;

        [ObservableProperty]
        private string _mileage = string.Empty;

        [ObservableProperty]
        private string _transmission = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        private long _carId;

        public CarDetailsPageViewModel(CarService carService)
        {
            _carService = carService;
        }

        public void Initialize(long carId)
        {
            _carId = carId;
            LoadCarDetailsCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task LoadCarDetailsAsync()
        {
            try
            {
                Car = await _carService.GetCarByIdAsync(_carId);
                if (Car != null)
                {
                    var brandName = Car.Model?.Brand?.Name ?? "Неизвестно";
                    var modelName = Car.Model?.Name ?? "Неизвестно";
                    CarName = $"{brandName} {modelName} ({Car.Year})";
                    Price = $"{Car.Cost:N0} ₽";
                    Brand = brandName;
                    Model = modelName;
                    Year = Car.Year?.ToString() ?? "—";
                    Color = Car.Color ?? "—";
                    Engine = Car.EngType?.Name ?? "—";
                    EngineVolume = Car.EngVol.HasValue ? $"{Car.EngVol:F1} л" : "—";
                    Mileage = Car.Mileage.HasValue ? $"{Car.Mileage:N0} км" : "—";
                    Transmission = Car.Transmission?.Name ?? "—";
                    Description = $"Тип: {Car.Type?.Name ?? "—"}\n" +
                                 $"Привод: {Car.Wd?.Name ?? "—"}\n" +
                                 $"Мощность: {(Car.Power.HasValue ? $"{Car.Power} л.с." : "—")}\n" +
                                 $"Состояние: {Car.Condition?.Name ?? "—"}";
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task EditCarAsync()
        {
            await Shell.Current.GoToAsync($"AddEditCarPage?carId={_carId}");
        }

        [RelayCommand]
        private async Task CreateSaleAsync()
        {
            await Shell.Current.GoToAsync($"CreateSalePage?carId={_carId}");
        }

        [RelayCommand]
        private async Task DeleteCarAsync()
        {
            bool answer = await Shell.Current.DisplayAlert(
                "Подтверждение",
                "Вы уверены, что хотите удалить этот автомобиль?",
                "Да",
                "Нет");

            if (answer)
            {
                try
                {
                    await _carService.DeleteCarAsync(_carId);
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Ошибка", $"Не удалось удалить автомобиль: {ex.Message}", "OK");
                }
            }
        }
    }
}

