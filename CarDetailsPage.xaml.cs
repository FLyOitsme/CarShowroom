using CarShowroom.ViewModels;
using Microsoft.Maui.Controls;

namespace CarShowroom
{
    [QueryProperty(nameof(CarId), "carId")]
    public partial class CarDetailsPage : ContentPage
    {
        public CarDetailsPageViewModel ViewModel { get; }

        public string CarId { get; set; } = string.Empty;

        public CarDetailsPage(CarDetailsPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (long.TryParse(CarId, out long carId))
            {
                ViewModel.Initialize(carId);
            }
        }
    }
}
