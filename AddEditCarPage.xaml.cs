using CarShowroom.Models;
using CarShowroom.Services;

namespace CarShowroom
{
    public partial class AddEditCarPage : ContentPage
    {
        private readonly CarService _carService;
        private readonly int? _carId;
        private bool _isEditMode => _carId.HasValue;

        public AddEditCarPage(int? carId = null)
        {
            InitializeComponent();
            _carService = new CarService();
            _carId = carId;

            if (_isEditMode)
            {
                Title = "Редактировать автомобиль";
                LoadCarData();
            }
        }

        private void LoadCarData()
        {
            if (_carId.HasValue)
            {
                var car = _carService.GetCarById(_carId.Value);
                if (car != null)
                {
                    BrandEntry.Text = car.Brand;
                    ModelEntry.Text = car.Model;
                    YearEntry.Text = car.Year.ToString();
                    ColorEntry.Text = car.Color;
                    PriceEntry.Text = car.Price.ToString();
                    EngineTypePicker.SelectedItem = car.EngineType;
                    EngineVolumeEntry.Text = car.EngineVolume.ToString();
                    MileageEntry.Text = car.Mileage.ToString();
                    TransmissionPicker.SelectedItem = car.Transmission;
                    DescriptionEditor.Text = car.Description;
                    CarImage.Source = car.ImageUrl;
                    VinEntry.Text = car.Vin;
                    EngineNumberEntry.Text = car.EngineNumber;
                    BodyNumberEntry.Text = car.BodyNumber;
                    RegistrationNumberEntry.Text = car.RegistrationNumber;
                }
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(BrandEntry.Text) ||
                string.IsNullOrWhiteSpace(ModelEntry.Text) ||
                string.IsNullOrWhiteSpace(YearEntry.Text) ||
                string.IsNullOrWhiteSpace(ColorEntry.Text) ||
                string.IsNullOrWhiteSpace(PriceEntry.Text) ||
                EngineTypePicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(EngineVolumeEntry.Text) ||
                string.IsNullOrWhiteSpace(MileageEntry.Text) ||
                TransmissionPicker.SelectedItem == null)
            {
                await DisplayAlert("Ошибка", "Пожалуйста, заполните все обязательные поля", "OK");
                return;
            }

            if (!int.TryParse(YearEntry.Text, out int year) || year < 1900 || year > DateTime.Now.Year + 1)
            {
                await DisplayAlert("Ошибка", "Введите корректный год", "OK");
                return;
            }

            if (!decimal.TryParse(PriceEntry.Text, out decimal price) || price <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректную цену", "OK");
                return;
            }

            if (!double.TryParse(EngineVolumeEntry.Text, out double engineVolume) || engineVolume <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректный объем двигателя", "OK");
                return;
            }

            if (!int.TryParse(MileageEntry.Text, out int mileage) || mileage < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректный пробег", "OK");
                return;
            }

            var car = new Car
            {
                Brand = BrandEntry.Text.Trim(),
                Model = ModelEntry.Text.Trim(),
                Year = year,
                Color = ColorEntry.Text.Trim(),
                Price = price,
                EngineType = EngineTypePicker.SelectedItem.ToString() ?? "Бензин",
                EngineVolume = engineVolume,
                Mileage = mileage,
                Transmission = TransmissionPicker.SelectedItem.ToString() ?? "Механика",
                Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
                ImageUrl = "dotnet_bot.png",
                Vin = VinEntry.Text?.Trim() ?? string.Empty,
                EngineNumber = EngineNumberEntry.Text?.Trim() ?? string.Empty,
                BodyNumber = BodyNumberEntry.Text?.Trim() ?? string.Empty,
                RegistrationNumber = RegistrationNumberEntry.Text?.Trim() ?? string.Empty
            };

            if (_isEditMode && _carId.HasValue)
            {
                car.Id = _carId.Value;
                _carService.UpdateCar(car);
            }
            else
            {
                _carService.AddCar(car);
            }

            await Navigation.PopAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
