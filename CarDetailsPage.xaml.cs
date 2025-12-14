using CarShowroom.Services;

namespace CarShowroom
{
    public partial class CarDetailsPage : ContentPage
    {
        private readonly CarService _carService;
        private readonly int _carId;

        public CarDetailsPage(int carId)
        {
            InitializeComponent();
            _carService = new CarService();
            _carId = carId;
            LoadCarDetails();
        }

        private void LoadCarDetails()
        {
            var car = _carService.GetCarById(_carId);
            if (car != null)
            {
                CarNameLabel.Text = car.FullName;
                PriceLabel.Text = $"{car.Price:N0} ₽";
                BrandLabel.Text = car.Brand;
                ModelLabel.Text = car.Model;
                YearLabel.Text = car.Year.ToString();
                ColorLabel.Text = car.Color;
                EngineLabel.Text = car.EngineType;
                EngineVolumeLabel.Text = $"{car.EngineVolume:F1} л";
                MileageLabel.Text = $"{car.Mileage:N0} км";
                TransmissionLabel.Text = car.Transmission;
                DescriptionLabel.Text = car.Description;
                CarImage.Source = car.ImageUrl;
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditCarPage(_carId));
        }

        private async void OnCreateContractClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SaleContractPage(_carId));
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
                _carService.DeleteCar(_carId);
                await Navigation.PopAsync();
            }
        }
    }
}
