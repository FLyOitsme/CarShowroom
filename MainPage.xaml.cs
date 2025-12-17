using CarShowroom.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CarShowroom
{
    public partial class MainPage : ContentPage
    {
        public MainPageViewModel ViewModel { get; }

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.LoadCarsCommand.ExecuteAsync(null);
        }

        private async void OnCarSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is DataLayer.Entities.Car selectedCar)
            {
                await ViewModel.CarSelectedCommand.ExecuteAsync(selectedCar);
            }
        }
    }
}
