using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Services;
using DataLayer.Entities;

namespace CarShowroom.ViewModels
{
    public partial class CreateSalePageViewModel : ObservableObject
    {
        private readonly CarService _carService;
        private readonly SaleService _saleService;
        private readonly UserService _userService;

        [ObservableProperty]
        private List<CarDisplayItem> _cars = new();

        [ObservableProperty]
        private List<ManagerDisplayItem> _managers = new();

        [ObservableProperty]
        private List<Addition> _additions = new();

        [ObservableProperty]
        private List<Discount> _discounts = new();

        [ObservableProperty]
        private List<User> _foundClients = new();

        [ObservableProperty]
        private List<User> _allClients = new();

        [ObservableProperty]
        private CarDisplayItem? _selectedCar;

        [ObservableProperty]
        private ManagerDisplayItem? _selectedManager;

        [ObservableProperty]
        private User? _selectedClient;

        [ObservableProperty]
        private string _clientSearchText = string.Empty;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _clientPhone = string.Empty;

        [ObservableProperty]
        private string _clientAddress = string.Empty;

        [ObservableProperty]
        private DateTime _saleDate = DateTime.Now;

        [ObservableProperty]
        private string _price = string.Empty;

        [ObservableProperty]
        private string _finalPrice = "Итоговая цена: 0 ₽";

        [ObservableProperty]
        private bool _isClientsVisible = false;

        [ObservableProperty]
        private bool _showAllClients = false;

        private long? _carId;
        private List<Addition> _selectedAdditions = new();
        private List<Discount> _selectedDiscounts = new();

        public CreateSalePageViewModel(CarService carService, SaleService saleService, UserService userService)
        {
            _carService = carService;
            _saleService = saleService;
            _userService = userService;
        }

        public void Initialize(long? carId = null)
        {
            _carId = carId;
            LoadDataCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                // Загрузка автомобилей
                var allCars = await _carService.GetAllCarsAsync();
                Cars = allCars.Select(c => new CarDisplayItem
                {
                    Car = c,
                    DisplayText = $"{c.Model?.Brand?.Name ?? "Неизвестно"} {c.Model?.Name ?? "Неизвестно"} ({c.Year})"
                }).ToList();

                if (_carId.HasValue)
                {
                    var selectedCarDisplay = Cars.FirstOrDefault(c => c.Car.Id == _carId.Value);
                    if (selectedCarDisplay != null)
                    {
                        SelectedCar = selectedCarDisplay;
                        Price = selectedCarDisplay.Car.Cost?.ToString("F0") ?? "0";
                    }
                }

                // Загрузка менеджеров
                var allManagers = await _userService.GetAllManagersAsync();
                Managers = allManagers.Select(m => new ManagerDisplayItem
                {
                    Manager = m,
                    DisplayText = $"{m.Surname} {m.Name} {m.Patronyc}".Trim()
                }).ToList();

                // Загрузка всех клиентов
                AllClients = await _userService.GetAllClientsAsync();

                // Загрузка дополнительных опций
                Additions = await _saleService.GetAllAdditionsAsync();

                // Загрузка скидок
                Discounts = await _saleService.GetAllDiscountsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        partial void OnPriceChanged(string value)
        {
            UpdateFinalPriceCommand.ExecuteAsync(null);
        }

        partial void OnSelectedCarChanged(CarDisplayItem? value)
        {
            if (value != null)
            {
                Price = value.Car.Cost?.ToString("F0") ?? "0";
            }
        }

        partial void OnSelectedClientChanged(User? value)
        {
            if (value != null)
            {
                var fullName = $"{value.Surname} {value.Name} {value.Patronyc}".Trim();
                ClientName = fullName;
                ClientPhone = value.Login ?? string.Empty;
                IsClientsVisible = false;
                ShowAllClients = false;
                ClientSearchText = string.Empty;
            }
        }

        [RelayCommand]
        private async Task SearchClientsAsync()
        {
            var searchText = ClientSearchText?.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Если поиск пустой, показываем всех клиентов
                FoundClients = AllClients;
                IsClientsVisible = FoundClients.Any();
                return;
            }

            if (searchText.Length < 2)
            {
                IsClientsVisible = false;
                return;
            }

            try
            {
                FoundClients = await _userService.SearchClientsAsync(searchText);
                IsClientsVisible = FoundClients.Any();
            }
            catch
            {
                IsClientsVisible = false;
            }
        }

        [RelayCommand]
        private void ToggleAllClients()
        {
            ShowAllClients = !ShowAllClients;
            if (ShowAllClients)
            {
                FoundClients = AllClients;
                IsClientsVisible = FoundClients.Any();
            }
            else
            {
                IsClientsVisible = false;
            }
        }

        [RelayCommand]
        private void SelectClient(User? client)
        {
            // Логика выбора клиента обрабатывается в OnSelectedClientChanged
            SelectedClient = client;
        }

        [RelayCommand]
        private async Task FindClientAsync()
        {
            if (string.IsNullOrWhiteSpace(ClientName))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите ФИО клиента для поиска", "OK");
                return;
            }

            try
            {
                var client = await _userService.SearchClientByNameAsync(ClientName);
                if (client != null)
                {
                    SelectedClient = client;
                    var fullName = $"{client.Surname} {client.Name} {client.Patronyc}".Trim();
                    ClientName = fullName;
                    ClientPhone = client.Login ?? string.Empty;
                    await Shell.Current.DisplayAlert("Успех", "Клиент найден", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Информация", "Клиент не найден. Будет создан новый клиент при сохранении.", "OK");
                    SelectedClient = null;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Ошибка поиска: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task UpdateFinalPriceAsync()
        {
            if (!decimal.TryParse(Price, out decimal basePrice) || basePrice <= 0)
            {
                FinalPrice = "Итоговая цена: 0 ₽";
                return;
            }

            // Добавляем стоимость дополнительных опций
            decimal additionsCost = 0;
            foreach (var addition in _selectedAdditions)
            {
                if (addition.Cost.HasValue)
                {
                    additionsCost += (decimal)addition.Cost.Value;
                }
            }

            var priceWithAdditions = basePrice + additionsCost;

            // Применяем скидки
            var selectedDiscountIds = _selectedDiscounts.Select(d => d.Id).ToList();

            try
            {
                var finalPrice = await _saleService.CalculateFinalPriceAsync(priceWithAdditions, selectedDiscountIds);
                FinalPrice = $"Итоговая цена: {finalPrice:N0} ₽";
                if (additionsCost > 0)
                {
                    FinalPrice += $"\n(базовая: {basePrice:N0} ₽ + опции: {additionsCost:N0} ₽)";
                }
            }
            catch
            {
                FinalPrice = "Итоговая цена: 0 ₽";
            }
        }

        public void OnAdditionSelectionChanged(IEnumerable<object> selectedItems)
        {
            _selectedAdditions = selectedItems.OfType<Addition>().ToList();
            UpdateFinalPriceCommand.ExecuteAsync(null);
        }

        public void OnDiscountSelectionChanged(IEnumerable<object> selectedItems)
        {
            _selectedDiscounts = selectedItems.OfType<Discount>().ToList();
            UpdateFinalPriceCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task CreateSaleAsync()
        {
            // Валидация
            if (SelectedCar == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Выберите автомобиль", "OK");
                return;
            }

            if (SelectedManager == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Выберите менеджера", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ClientName))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите ФИО клиента", "OK");
                return;
            }

            if (!decimal.TryParse(Price, out decimal price) || price <= 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите корректную цену", "OK");
                return;
            }

            try
            {
                // Создаем или получаем клиента
                User client;
                if (SelectedClient != null)
                {
                    client = SelectedClient;
                }
                else
                {
                    client = await _userService.CreateOrGetClientAsync(
                        ClientName.Trim(),
                        ClientPhone?.Trim(),
                        ClientAddress?.Trim());
                }

                // Рассчитываем итоговую цену с опциями и скидками
                decimal additionsCost = 0;
                foreach (var addition in _selectedAdditions)
                {
                    if (addition.Cost.HasValue)
                    {
                        additionsCost += (decimal)addition.Cost.Value;
                    }
                }

                var priceWithAdditions = price + additionsCost;
                var selectedDiscountIds = _selectedDiscounts.Select(d => d.Id).ToList();
                var finalPrice = await _saleService.CalculateFinalPriceAsync(priceWithAdditions, selectedDiscountIds);

                // Создаем продажу
                var sale = new Sale
                {
                    CarId = SelectedCar.Car.Id,
                    ManagerId = SelectedManager.Manager.Id,
                    Date = DateOnly.FromDateTime(SaleDate),
                    Cost = (float)finalPrice
                };

                var selectedAdditionIds = _selectedAdditions.Select(a => a.Id).ToList();

                // Сохраняем продажу
                await _saleService.CreateSaleAsync(sale, selectedAdditionIds, selectedDiscountIds);

                await Shell.Current.DisplayAlert("Успех", "Продажа успешно создана", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось создать продажу: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    public class CarDisplayItem
    {
        public Car Car { get; set; } = null!;
        public string DisplayText { get; set; } = string.Empty;
    }

    public class ManagerDisplayItem
    {
        public User Manager { get; set; } = null!;
        public string DisplayText { get; set; } = string.Empty;
    }
}

