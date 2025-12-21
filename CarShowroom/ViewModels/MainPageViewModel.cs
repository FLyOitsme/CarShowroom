using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using Dom;

namespace CarShowroom.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly ICarService _carService;

        [ObservableProperty]
        private List<Car> _cars = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private Car? _selectedCar;

        public MainPageViewModel(ICarService carService)
        {
            _carService = carService;
        }

        [RelayCommand]
        private async Task LoadCarsAsync()
        {
            try
            {
                Cars = await _carService.GetAllCarsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить автомобили: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SearchCarsAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    Cars = await _carService.GetAllCarsAsync();
                }
                else
                {
                    Cars = await _carService.SearchCarsAsync(SearchText);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Ошибка поиска: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task CarSelectedAsync(Car? car)
        {
            if (car != null)
            {
                await Shell.Current.GoToAsync($"///MainPage/CarDetailsPage?carId={car.Id}");
                SelectedCar = null;
            }
        }

        [RelayCommand]
        private async Task AddCarAsync()
        {
            await Shell.Current.GoToAsync("///MainPage/AddEditCarPage");
        }

        partial void OnSearchTextChanged(string value)
        {
            SearchCarsCommand.ExecuteAsync(null);
        }
    }
}

