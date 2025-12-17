using CarShowroom.Services;
using DataLayer.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CarShowroom
{
    public partial class CreateSalePage : ContentPage
    {
        private CarService? _carService;
        private SaleService? _saleService;
        private UserService? _userService;
        private long? _carId;
        private List<Addition> _additions = new();
        private List<Discount> _discounts = new();
        private User? _selectedClient;

        public CreateSalePage(long? carId = null)
        {
            InitializeComponent();
            _carId = carId;
            SaleDatePicker.Date = DateTime.Now;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            // Используем Dispatcher для получения сервисов после полной загрузки
            Dispatcher.DispatchAsync(async () =>
            {
                await InitializeServicesAsync();
            });
        }

        private async Task InitializeServicesAsync()
        {
            // Ждем, пока Handler полностью инициализирован
            var maxAttempts = 10;
            var attempt = 0;
            
            while ((_carService == null || _saleService == null || _userService == null) && attempt < maxAttempts)
            {
                if (Handler?.MauiContext?.Services != null)
                {
                    _carService = Handler.MauiContext.Services.GetService<CarService>();
                    _saleService = Handler.MauiContext.Services.GetService<SaleService>();
                    _userService = Handler.MauiContext.Services.GetService<UserService>();
                    
                    if (_carService != null && _saleService != null && _userService != null)
                    {
                        await LoadDataAsync();
                        return;
                    }
                }
                
                await Task.Delay(50);
                attempt++;
            }
            
            if (_carService == null || _saleService == null || _userService == null)
            {
                await DisplayAlert("Ошибка", "Не все сервисы найдены", "OK");
                await Navigation.PopAsync();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Если сервисы еще не получены, пытаемся получить
            if (_carService == null || _saleService == null || _userService == null)
            {
                await InitializeServicesAsync();
            }
            else
            {
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            if (_carService == null || _saleService == null || _userService == null)
            {
                await DisplayAlert("Ошибка", "Сервисы не инициализированы", "OK");
                return;
            }

            try
            {
                // Загрузка автомобилей
                var cars = await _carService.GetAllCarsAsync();
                // Создаем список для отображения с форматированием
                var carDisplayList = cars.Select(c => new
                {
                    Car = c,
                    DisplayText = $"{c.Model?.Brand?.Name ?? "Неизвестно"} {c.Model?.Name ?? "Неизвестно"} ({c.Year})"
                }).ToList();
                
                CarPicker.ItemsSource = carDisplayList;
                CarPicker.ItemDisplayBinding = new Binding("DisplayText");
                
                if (_carId.HasValue)
                {
                    var selectedCarDisplay = carDisplayList.FirstOrDefault(c => c.Car.Id == _carId.Value);
                    if (selectedCarDisplay != null)
                    {
                        CarPicker.SelectedItem = selectedCarDisplay;
                        PriceEntry.Text = selectedCarDisplay.Car.Cost?.ToString("F0") ?? "0";
                    }
                }

                // Загрузка менеджеров
                var managers = await _userService.GetAllManagersAsync();
                var managerDisplayList = managers.Select(m => new
                {
                    Manager = m,
                    DisplayText = $"{m.Surname} {m.Name} {m.Patronyc}".Trim()
                }).ToList();
                
                ManagerPicker.ItemsSource = managerDisplayList;
                ManagerPicker.ItemDisplayBinding = new Binding("DisplayText");

                // Загрузка дополнительных опций
                _additions = await _saleService.GetAllAdditionsAsync();
                AdditionsCollectionView.ItemsSource = _additions;

                // Загрузка скидок
                _discounts = await _saleService.GetAllDiscountsAsync();
                DiscountsCollectionView.ItemsSource = _discounts;

                // Обновление итоговой цены при изменении
                PriceEntry.TextChanged += OnPriceChanged;
                DiscountsCollectionView.SelectionChanged += OnDiscountsChanged;
                AdditionsCollectionView.SelectionChanged += OnAdditionsChanged;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        private void OnPriceChanged(object? sender, TextChangedEventArgs e)
        {
            UpdateFinalPrice();
        }

        private void OnDiscountsChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateFinalPrice();
        }

        private void OnAdditionsChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateFinalPrice();
        }

        private async void UpdateFinalPrice()
        {
            if (!decimal.TryParse(PriceEntry.Text, out decimal basePrice) || basePrice <= 0)
            {
                FinalPriceLabel.Text = "Итоговая цена: 0 ₽";
                return;
            }

            // Добавляем стоимость дополнительных опций
            decimal additionsCost = 0;
            if (AdditionsCollectionView.SelectedItems != null)
            {
                foreach (var item in AdditionsCollectionView.SelectedItems)
                {
                    if (item is Addition addition && addition.Cost.HasValue)
                    {
                        additionsCost += (decimal)addition.Cost.Value;
                    }
                }
            }

            var priceWithAdditions = basePrice + additionsCost;

            // Применяем скидки
            var selectedDiscounts = new List<int>();
            if (DiscountsCollectionView.SelectedItems != null)
            {
                foreach (var item in DiscountsCollectionView.SelectedItems)
                {
                    if (item is Discount discount)
                    {
                        selectedDiscounts.Add(discount.Id);
                    }
                }
            }

            if (_saleService == null) return;

            try
            {
                var finalPrice = await _saleService.CalculateFinalPriceAsync(priceWithAdditions, selectedDiscounts);
                FinalPriceLabel.Text = $"Итоговая цена: {finalPrice:N0} ₽";
                if (additionsCost > 0)
                {
                    FinalPriceLabel.Text += $"\n(базовая: {basePrice:N0} ₽ + опции: {additionsCost:N0} ₽)";
                }
            }
            catch
            {
                FinalPriceLabel.Text = "Итоговая цена: 0 ₽";
            }
        }

        private async void OnClientSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.Trim();
            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
            {
                ClientsCollectionView.IsVisible = false;
                return;
            }

            if (_userService == null) return;

            try
            {
                var clients = await _userService.SearchClientsAsync(searchText);
                ClientsCollectionView.ItemsSource = clients;
                ClientsCollectionView.IsVisible = clients.Any();
            }
            catch
            {
                ClientsCollectionView.IsVisible = false;
            }
        }

        private void OnClientSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is User selectedClient)
            {
                _selectedClient = selectedClient;
                var fullName = $"{selectedClient.Surname} {selectedClient.Name} {selectedClient.Patronyc}".Trim();
                ClientNameEntry.Text = fullName;
                ClientPhoneEntry.Text = selectedClient.Login;
                ClientsCollectionView.IsVisible = false;
                ClientSearchBar.Text = string.Empty;
                ClientsCollectionView.SelectedItem = null;
            }
        }

        private void OnSearchClientClicked(object sender, EventArgs e)
        {
            OnClientSearchTextChanged(sender, new TextChangedEventArgs(ClientSearchBar.Text, ClientSearchBar.Text));
        }

        private async void OnFindClientClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClientNameEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите ФИО клиента для поиска", "OK");
                return;
            }

            if (_userService == null) return;

            try
            {
                var client = await _userService.SearchClientByNameAsync(ClientNameEntry.Text);
                if (client != null)
                {
                    _selectedClient = client;
                    var fullName = $"{client.Surname} {client.Name} {client.Patronyc}".Trim();
                    ClientNameEntry.Text = fullName;
                    ClientPhoneEntry.Text = client.Login;
                    await DisplayAlert("Успех", "Клиент найден", "OK");
                }
                else
                {
                    await DisplayAlert("Информация", "Клиент не найден. Будет создан новый клиент при сохранении.", "OK");
                    _selectedClient = null;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка поиска: {ex.Message}", "OK");
            }
        }

        private async void OnCreateSaleClicked(object sender, EventArgs e)
        {
            if (_carService == null || _saleService == null || _userService == null)
            {
                await DisplayAlert("Ошибка", "Сервисы не инициализированы", "OK");
                return;
            }

            // Валидация
            if (CarPicker.SelectedItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите автомобиль", "OK");
                return;
            }

            if (ManagerPicker.SelectedItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите менеджера", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ClientNameEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите ФИО клиента", "OK");
                return;
            }

            if (!decimal.TryParse(PriceEntry.Text, out decimal price) || price <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректную цену", "OK");
                return;
            }

            try
            {
                if (CarPicker.SelectedItem == null || ManagerPicker.SelectedItem == null)
                {
                    await DisplayAlert("Ошибка", "Выберите автомобиль и менеджера", "OK");
                    return;
                }

                // Получаем выбранные объекты из оберток
                dynamic carWrapper = CarPicker.SelectedItem;
                dynamic managerWrapper = ManagerPicker.SelectedItem;
                var car = (Car)carWrapper.Car;
                var manager = (User)managerWrapper.Manager;

                // Создаем или получаем клиента
                User client;
                if (_selectedClient != null)
                {
                    client = _selectedClient;
                }
                else
                {
                    client = await _userService.CreateOrGetClientAsync(
                        ClientNameEntry.Text.Trim(),
                        ClientPhoneEntry.Text?.Trim(),
                        ClientAddressEditor.Text?.Trim());
                }

                // Создаем продажу
                var sale = new Sale
                {
                    CarId = car.Id,
                    ManagerId = manager.Id,
                    Date = DateOnly.FromDateTime(SaleDatePicker.Date),
                    Cost = (float)price
                };

                // Получаем выбранные опции и скидки
                var selectedAdditions = new List<int>();
                if (AdditionsCollectionView.SelectedItems != null)
                {
                    foreach (var item in AdditionsCollectionView.SelectedItems)
                    {
                        if (item is Addition addition)
                        {
                            selectedAdditions.Add(addition.Id);
                        }
                    }
                }

                var selectedDiscounts = new List<int>();
                if (DiscountsCollectionView.SelectedItems != null)
                {
                    foreach (var item in DiscountsCollectionView.SelectedItems)
                    {
                        if (item is Discount discount)
                        {
                            selectedDiscounts.Add(discount.Id);
                        }
                    }
                }

                // Рассчитываем итоговую цену с опциями и скидками
                decimal additionsCost = 0;
                foreach (var addId in selectedAdditions)
                {
                    var addition = _additions.FirstOrDefault(a => a.Id == addId);
                    if (addition?.Cost.HasValue == true)
                    {
                        additionsCost += (decimal)addition.Cost.Value;
                    }
                }

                var priceWithAdditions = price + additionsCost;
                var finalPrice = await _saleService.CalculateFinalPriceAsync(priceWithAdditions, selectedDiscounts);
                sale.Cost = (float)finalPrice;

                // Сохраняем продажу
                await _saleService.CreateSaleAsync(sale, selectedAdditions, selectedDiscounts);

                await DisplayAlert("Успех", "Продажа успешно создана", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось создать продажу: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}

