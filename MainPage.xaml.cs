using CarShowroom.Models;
using CarShowroom.Services;

namespace CarShowroom
{
    public partial class MainPage : ContentPage
    {
        private readonly CarService _carService;
        private List<Car> _allCars;

        public MainPage()
        {
            InitializeComponent();
            _carService = new CarService();
            LoadCars();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadCars();
        }

        private void LoadCars()
        {
            _allCars = _carService.GetAllCars();
            CarsCollectionView.ItemsSource = _allCars;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                CarsCollectionView.ItemsSource = _allCars;
            }
            else
            {
                var filteredCars = _allCars.Where(car =>
                    car.Brand.ToLower().Contains(searchText) ||
                    car.Model.ToLower().Contains(searchText) ||
                    car.Color.ToLower().Contains(searchText) ||
                    car.Description.ToLower().Contains(searchText)
                ).ToList();
                
                CarsCollectionView.ItemsSource = filteredCars;
            }
        }

        private async void OnCarSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Car selectedCar)
            {
                await Navigation.PushAsync(new CarDetailsPage(selectedCar.Id));
                CarsCollectionView.SelectedItem = null;
            }
        }

        private async void OnCarTapped(object sender, EventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is Car car)
            {
                await Navigation.PushAsync(new CarDetailsPage(car.Id));
            }
        }

        private async void OnAddCarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditCarPage());
        }
    }
}
