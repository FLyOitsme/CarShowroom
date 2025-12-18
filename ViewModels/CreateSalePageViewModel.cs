using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using DataLayer.Entities;
using CommunityToolkit.Maui.Storage;

namespace CarShowroom.ViewModels
{
    public partial class CreateSalePageViewModel : ObservableObject
    {
        private readonly ICarService _carService;
        private readonly ISaleService _saleService;
        private readonly IUserService _userService;
        private readonly IPdfContractService _pdfContractService;
        private readonly IFileSaver? _fileSaver;

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

        public CreateSalePageViewModel(ICarService carService, ISaleService saleService, IUserService userService, IPdfContractService pdfContractService, IFileSaver? fileSaver = null)
        {
            _carService = carService;
            _saleService = saleService;
            _userService = userService;
            _pdfContractService = pdfContractService;
            _fileSaver = fileSaver;
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

                // Проверяем, что все данные корректны
                if (SelectedCar?.Car?.Id == null || SelectedCar.Car.Id == 0)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не выбран автомобиль", "OK");
                    return;
                }

                if (SelectedManager?.Manager?.Id == null || SelectedManager.Manager.Id == 0)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не выбран менеджер", "OK");
                    return;
                }

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
                var createdSale = await _saleService.CreateSaleAsync(sale, selectedAdditionIds, selectedDiscountIds);

                // Предлагаем сгенерировать PDF договор
                var generatePdf = await Shell.Current.DisplayAlert(
                    "Успех",
                    "Продажа успешно создана. Сгенерировать PDF договор?",
                    "Да",
                    "Нет");

                if (generatePdf)
                {
                    await GenerateContractAsync(createdSale, client, finalPrice, price);
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Не удалось создать продажу: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nДетали: {ex.InnerException.Message}";
                }
                await Shell.Current.DisplayAlert("Ошибка", errorMessage, "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async Task GenerateContractAsync(Sale sale, User client, decimal finalPrice, decimal basePrice)
        {
            try
            {
                // Получаем полную информацию о продаже с AsNoTracking
                var fullSale = await _saleService.GetSaleByIdAsync(sale.Id);
                if (fullSale == null || fullSale.Car == null)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить данные о продаже", "OK");
                    return;
                }

                // Получаем опции и скидки
                var additions = await _saleService.GetSaleAdditionsAsync(sale.Id);
                var discounts = await _saleService.GetSaleDiscountsAsync(sale.Id);

                // Генерируем PDF
                var pdfBytes = _pdfContractService.GenerateContract(
                    fullSale,
                    client,
                    SelectedManager!.Manager,
                    fullSale.Car,
                    additions,
                    discounts,
                    basePrice,
                    finalPrice);

                // Сохраняем PDF
                await SavePdfFileAsync(pdfBytes, $"Договор_№{sale.Id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сгенерировать PDF: {ex.Message}", "OK");
            }
        }

        private async Task SavePdfFileAsync(byte[] pdfBytes, string fileName)
        {
            try
            {
                // Используем FileSaver для выбора места сохранения, если доступен
                if (_fileSaver != null)
                {
                    using var stream = new MemoryStream(pdfBytes);
                    var cancellationToken = CancellationToken.None;
                    var fileSaverResult = await _fileSaver.SaveAsync(fileName, stream, cancellationToken);
                    
                    if (fileSaverResult.IsSuccessful)
                    {
                        await Shell.Current.DisplayAlert("Успех", 
                            $"PDF договор сохранен:\n{fileSaverResult.FilePath}\n\n" +
                            $"Размер файла: {pdfBytes.Length / 1024} КБ", 
                            "OK");
                        return;
                    }
                    else
                    {
                        // Если пользователь отменил выбор или произошла ошибка
                        if (fileSaverResult.Exception != null)
                        {
                            // Если пользователь отменил, просто сообщаем об этом
                            if (fileSaverResult.Exception.Message.Contains("cancel", StringComparison.OrdinalIgnoreCase) ||
                                fileSaverResult.Exception.Message.Contains("отмен", StringComparison.OrdinalIgnoreCase))
                            {
                                await Shell.Current.DisplayAlert("Информация", 
                                    "Сохранение файла отменено пользователем", 
                                    "OK");
                                return;
                            }
                            else
                            {
                                // При другой ошибке сохраняем в кэш как резервный вариант
                                await SaveToCacheAsync(pdfBytes, fileName, fileSaverResult.Exception.Message);
                                return;
                            }
                        }
                        else
                        {
                            await SaveToCacheAsync(pdfBytes, fileName, "Неизвестная ошибка");
                            return;
                        }
                    }
                }
                else
                {
                    // Если FileSaver не доступен, сохраняем в кэш
                    await SaveToCacheAsync(pdfBytes, fileName, "FileSaver не доступен");
                }
            }
            catch (Exception ex)
            {
                // Если FileSaver не доступен, сохраняем в кэш
                await SaveToCacheAsync(pdfBytes, fileName, ex.Message);
            }
        }

        private async Task SaveToCacheAsync(byte[] pdfBytes, string fileName, string errorMessage)
        {
            try
            {
                var cacheDir = FileSystem.CacheDirectory;
                var filePath = Path.Combine(cacheDir, fileName);
                await File.WriteAllBytesAsync(filePath, pdfBytes);
                
                await Shell.Current.DisplayAlert("Информация", 
                    $"PDF договор сохранен в кэш:\n{filePath}\n\n" +
                    $"Размер файла: {pdfBytes.Length / 1024} КБ\n\n" +
                    $"Примечание: {errorMessage}", 
                    "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", 
                    $"Не удалось сохранить PDF файл: {ex.Message}", 
                    "OK");
            }
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

