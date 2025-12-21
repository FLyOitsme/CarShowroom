using CarShowroom.ViewModels;
using Microsoft.Maui.Controls;

namespace CarShowroom
{
    [QueryProperty(nameof(CarId), "carId")]
    public partial class AddEditCarPage : ContentPage
    {
        public AddEditCarPageViewModel ViewModel { get; }

        public string CarId { get; set; } = string.Empty;

        public AddEditCarPage(AddEditCarPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            long? carId = null;
            if (!string.IsNullOrEmpty(CarId) && long.TryParse(CarId, out long id))
            {
                carId = id;
            }
            ViewModel.Initialize(carId);
        }
    }
}
