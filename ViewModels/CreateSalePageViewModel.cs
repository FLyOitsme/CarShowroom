using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using DataLayer.Entities;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

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
        private List<Discount> _applicableDiscounts = new();

        [ObservableProperty]
        private List<DiscountSelectionItem> _discountSelectionItems = new();

        [ObservableProperty]
        private List<Client> _foundClients = new();

        [ObservableProperty]
        private List<Client> _allClients = new();

        [ObservableProperty]
        private CarDisplayItem? _selectedCar;

        [ObservableProperty]
        private ManagerDisplayItem? _selectedManager;

        [ObservableProperty]
        private Client? _selectedClient;

        [ObservableProperty]
        private string _clientSearchText = string.Empty;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _clientPhone = string.Empty;

        [ObservableProperty]
        private string _clientPassData = string.Empty;

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

        [ObservableProperty]
        private Discount? _selectedDiscount;

        public bool IsCancelling { get; set; }
        
        [ObservableProperty]
        private bool _isNavigationLocked = true;

        [ObservableProperty]
        private string _carBrand = string.Empty;

        [ObservableProperty]
        private string _carModel = string.Empty;

        [ObservableProperty]
        private string _carYear = string.Empty;

        [ObservableProperty]
        private string _carColor = string.Empty;

        [ObservableProperty]
        private string _carEngine = string.Empty;

        [ObservableProperty]
        private string _carEngineVolume = string.Empty;

        [ObservableProperty]
        private string _carMileage = string.Empty;

        [ObservableProperty]
        private string _carTransmission = string.Empty;

        [ObservableProperty]
        private string _carType = string.Empty;

        [ObservableProperty]
        private string _carDrive = string.Empty;

        [ObservableProperty]
        private string _carPower = string.Empty;

        [ObservableProperty]
        private string _carCondition = string.Empty;

        [ObservableProperty]
        private bool _isCarInfoVisible = false;

        private long? _carId;
        private List<Addition> _selectedAdditions = new();
        private bool _discountsAutoApplied = false;
        private bool _isPageActive = false;
        private CancellationTokenSource? _cancellationTokenSource;

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
            // Отменяем предыдущие операции, если они есть
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            catch { }
            
            // Создаем новый CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();
            
            _isPageActive = true;
            _carId = carId;
            
            // Блокируем навигацию
            IsNavigationLocked = true;
            // Уведомляем главную страницу о блокировке
            MessagingCenter.Send(this, "NavigationLocked", true);
            
            // Асинхронно ждем немного перед началом загрузки, чтобы дать время предыдущим операциям завершиться
            _ = Task.Run(async () =>
            {
                try
                {
                    // Увеличиваем задержку, чтобы дать время всем операциям с БД завершиться
                    await Task.Delay(300); // Задержка для завершения предыдущих операций
                    if (_isPageActive && _cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            if (_isPageActive && _cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                LoadDataCommand.ExecuteAsync(null);
                            }
                        });
                    }
                }
                catch { }
            });
        }

        public void ClearData()
        {
            // Помечаем страницу как неактивной ПЕРЕД отменой операций,
            // чтобы предотвратить запуск новых операций
            _isPageActive = false;
            
            // Сбрасываем флаг проверки скидок, чтобы разблокировать любые ожидающие операции
            _isCheckingDiscounts = false;
            
            // Отменяем все активные операции с БД
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
            catch
            {
                // Игнорируем ошибки при отмене
            }
            
            // Очищаем все данные формы
            // Теперь partial методы не будут выполнять операции с БД благодаря проверке _isPageActive
            try
            {
                SelectedCar = null;
                SelectedManager = null;
                SelectedClient = null;
                ClientSearchText = string.Empty;
                ClientName = string.Empty;
                ClientPhone = string.Empty;
                ClientPassData = string.Empty;
                SaleDate = DateTime.Now;
                Price = string.Empty;
                FinalPrice = "Итоговая цена: 0 ₽";
                SelectedDiscount = null;
                ApplicableDiscounts.Clear();
                DiscountSelectionItems.Clear();
                FoundClients.Clear();
                IsClientsVisible = false;
                ShowAllClients = false;
                
                _carId = null;
                _selectedAdditions.Clear();
                _discountsAutoApplied = false;
                
                // Очищаем информацию об автомобиле
                IsCarInfoVisible = false;
                CarBrand = string.Empty;
                CarModel = string.Empty;
                CarYear = string.Empty;
                CarColor = string.Empty;
                CarEngine = string.Empty;
                CarEngineVolume = string.Empty;
                CarMileage = string.Empty;
                CarTransmission = string.Empty;
                CarType = string.Empty;
                CarDrive = string.Empty;
                CarPower = string.Empty;
                CarCondition = string.Empty;
                
                // Разблокируем навигацию при очистке данных
                IsNavigationLocked = false;
                MessagingCenter.Send(this, "NavigationLocked", false);
            }
            catch
            {
                // Игнорируем ошибки при очистке данных
            }
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (!_isPageActive || _cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested) return;
            
            try
            {
                // Загрузка автомобилей
                var allCars = await _carService.GetAllCarsAsync();
                if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
                
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
                if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
                Managers = allManagers.Select(m => new ManagerDisplayItem
                {
                    Manager = m,
                    DisplayText = $"{m.Surname} {m.Name} {m.Patronyc}".Trim()
                }).ToList();

                // Загрузка всех клиентов
                AllClients = await _userService.GetAllClientEntitiesAsync();
                if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;

                // Загрузка дополнительных опций
                Additions = await _saleService.GetAllAdditionsAsync();
                if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;

                // Загрузка скидок
                Discounts = await _saleService.GetAllDiscountsAsync();
                if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
                
                // Автоматически применяем подходящие скидки
                // Если автомобиль уже выбран, проверяем условия
                if (SelectedCar != null)
                {
                    await CheckAndApplyDiscountsAsync();
                }
                else
                {
                    // Если автомобиль не выбран, не показываем скидки
                    // Скидки будут показаны только после выбора автомобиля
                    ApplicableDiscounts.Clear();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }


        private async Task CheckAndApplyDiscountsAsync()
        {
            if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true || Discounts == null || !Discounts.Any() || SelectedCar == null)
                return;

            // Получаем количество покупок клиента
            int purchaseCount = 0;
            if (SelectedClient != null)
            {
                purchaseCount = await _saleService.GetClientPurchaseCountAsync(SelectedClient.Id, null).ConfigureAwait(false);
                if (!_isPageActive) return;
                // Небольшая задержка для завершения операции с БД
                await Task.Delay(50).ConfigureAwait(false);
                if (!_isPageActive) return;
            }
            else if (!string.IsNullOrWhiteSpace(ClientName))
            {
                purchaseCount = await _saleService.GetClientPurchaseCountAsync(null, ClientName).ConfigureAwait(false);
                if (!_isPageActive) return;
                // Небольшая задержка для завершения операции с БД
                await Task.Delay(50).ConfigureAwait(false);
                if (!_isPageActive) return;
            }

            var applicableDiscounts = new List<Discount>();
            
            // Определяем, выбран ли клиент (либо через SelectedClient, либо через ClientName)
            bool isClientSelected = SelectedClient != null || !string.IsNullOrWhiteSpace(ClientName);
            
            // Выполняем проверки последовательно, чтобы избежать конфликтов с DbContext
            foreach (var discount in Discounts)
            {
                try
                {
                    if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
                    
                    if (await IsDiscountApplicableAsync(discount, SelectedCar.Car, purchaseCount, isClientSelected).ConfigureAwait(false))
                    {
                        applicableDiscounts.Add(discount);
                    }
                    
                    // Небольшая задержка между проверками для избежания конфликтов с БД
                    await Task.Delay(10).ConfigureAwait(false);
                }
                catch
                {
                    // Пропускаем скидку при ошибке проверки
                    continue;
                }
            }
            
            // Обновляем список применимых скидок для отображения менеджеру
            ApplicableDiscounts = applicableDiscounts;
            
            // Обновляем список элементов выбора скидок
            UpdateDiscountSelectionItems(applicableDiscounts);
            
            // Выбираем только одну скидку - с наибольшим процентом
            if (applicableDiscounts.Any())
            {
                var bestDiscount = applicableDiscounts
                    .OrderByDescending(d => d.Cost ?? 0)
                    .First();
                SelectedDiscount = bestDiscount;
                
                // Обновляем выбор в элементах выбора
                foreach (var item in DiscountSelectionItems)
                {
                    item.IsSelected = item.Discount.Id == bestDiscount.Id;
                }
            }
            else
            {
                SelectedDiscount = null;
                
                // Снимаем выбор со всех элементов
                foreach (var item in DiscountSelectionItems)
                {
                    item.IsSelected = false;
                }
            }
            
            _discountsAutoApplied = true;
            
            // Проверяем, что страница все еще активна перед обновлением цены
            if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
            
            // Небольшая задержка перед обновлением цены, чтобы избежать параллельных запросов
            await Task.Delay(100).ConfigureAwait(false);
            if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
            
            // Вызываем обновление цены синхронно, чтобы избежать параллельных запросов
            await UpdateFinalPriceAsync().ConfigureAwait(false);
            
            // Проверяем еще раз после обновления цены
            if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
            
            // Уведомляем UI о необходимости обновить выбранные элементы
            OnDiscountsAutoApplied();
        }

        private async Task<bool> IsDiscountApplicableAsync(Discount discount, Car car, int clientPurchaseCount, bool isClientSelected)
        {
            // Проверяем срок действия акции
            if (discount.StartDate.HasValue || discount.EndDate.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (discount.StartDate.HasValue && today < discount.StartDate.Value)
                    return false; // Акция еще не началась
                if (discount.EndDate.HasValue && today > discount.EndDate.Value)
                    return false; // Акция уже закончилась
            }

            // Если описание пустое, скидка применяется ко всем автомобилям
            if (string.IsNullOrWhiteSpace(discount.Description))
            {
                return true;
            }

            var description = discount.Description.ToLower().Trim();
            
            // Проверяем, есть ли в описании условия, связанные с клиентом
            bool hasClientConditions = description.Contains("первый_клиент") || 
                                      description.Contains("первый клиент") || 
                                      description.Contains("первая покупка") ||
                                      description.Contains("vip_клиент") || 
                                      description.Contains("vip клиент") || 
                                      description.Contains("вип клиент") ||
                                      description.Contains("покупок>") || 
                                      description.Contains("покупок <") ||
                                      description.Contains("покупок<") ||
                                      description.Contains("покупок=") ||
                                      description.Contains("покупок>=") ||
                                      description.Contains("покупок<=");
            
            // Если скидка связана с клиентом, но клиент не выбран, скидка не применяется
            if (hasClientConditions && !isClientSelected)
            {
                return false;
            }
            
            // Парсим условия из описания
            // Формат: "цена>1000000; год<2020; тип=Седан; бренд=BMW,Audi; состояние=Новый; пробег<50000; первый_клиент; vip_клиент"
            // Используем точку с запятой и перенос строки для разделения основных условий, запятая используется для значений внутри условий
            
            var conditions = description.Split(new[] { ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var condition in conditions)
            {
                var trimmedCondition = condition.Trim().ToLower();
                
                // Проверка цены
                if (trimmedCondition.StartsWith("цена>") || trimmedCondition.StartsWith("цена >"))
                {
                    if (TryParseValue(trimmedCondition, "цена", out float minPrice))
                    {
                        if (car.Cost == null || car.Cost <= minPrice)
                            return false;
                    }
                }
                else if (trimmedCondition.StartsWith("цена<") || trimmedCondition.StartsWith("цена <"))
                {
                    if (TryParseValue(trimmedCondition, "цена", out float maxPrice))
                    {
                        if (car.Cost == null || car.Cost >= maxPrice)
                            return false;
                    }
                }
                else if (trimmedCondition.StartsWith("цена=") || trimmedCondition.StartsWith("цена ="))
                {
                    if (TryParseValue(trimmedCondition, "цена", out float exactPrice))
                    {
                        if (car.Cost == null || Math.Abs(car.Cost.Value - exactPrice) > 0.01f)
                            return false;
                    }
                }
                
                // Проверка года
                else if (trimmedCondition.StartsWith("год>") || trimmedCondition.StartsWith("год >"))
                {
                    if (TryParseValue(trimmedCondition, "год", out float minYear))
                    {
                        if (car.Year == null || car.Year <= minYear)
                            return false;
                    }
                }
                else if (trimmedCondition.StartsWith("год<") || trimmedCondition.StartsWith("год <"))
                {
                    if (TryParseValue(trimmedCondition, "год", out float maxYear))
                    {
                        if (car.Year == null || car.Year >= maxYear)
                            return false;
                    }
                }
                else if (trimmedCondition.StartsWith("год=") || trimmedCondition.StartsWith("год ="))
                {
                    if (TryParseValue(trimmedCondition, "год", out float exactYear))
                    {
                        if (car.Year == null || car.Year != exactYear)
                            return false;
                    }
                }
                
                // Проверка типа (поддерживаем множественные значения через запятую)
                else if (trimmedCondition.StartsWith("тип=") || trimmedCondition.StartsWith("тип ="))
                {
                    var typeNames = ExtractValue(trimmedCondition, "тип").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (typeNames.Length > 0)
                    {
                        var carTypeName = car.Type?.Name;
                        if (string.IsNullOrEmpty(carTypeName))
                            return false;
                        
                        bool matches = false;
                        foreach (var typeName in typeNames)
                        {
                            var trimmedTypeName = typeName.Trim();
                            if (carTypeName.Equals(trimmedTypeName, StringComparison.OrdinalIgnoreCase))
                            {
                                matches = true;
                                break;
                            }
                        }
                        if (!matches)
                            return false;
                    }
                }
                
                // Проверка бренда (поддерживаем множественные значения через запятую)
                else if (trimmedCondition.StartsWith("бренд=") || trimmedCondition.StartsWith("бренд ="))
                {
                    var brandNames = ExtractValue(trimmedCondition, "бренд").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (brandNames.Length > 0)
                    {
                        var carBrandName = car.Model?.Brand?.Name;
                        if (string.IsNullOrEmpty(carBrandName))
                            return false;
                        
                        bool matches = false;
                        foreach (var brandName in brandNames)
                        {
                            var trimmedBrandName = brandName.Trim();
                            if (carBrandName.Equals(trimmedBrandName, StringComparison.OrdinalIgnoreCase))
                            {
                                matches = true;
                                break;
                            }
                        }
                        if (!matches)
                            return false;
                    }
                }
                
                // Проверка состояния (поддерживаем множественные значения через запятую)
                else if (trimmedCondition.StartsWith("состояние=") || trimmedCondition.StartsWith("состояние ="))
                {
                    var conditionNames = ExtractValue(trimmedCondition, "состояние").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (conditionNames.Length > 0)
                    {
                        var carConditionName = car.Condition?.Name;
                        if (string.IsNullOrEmpty(carConditionName))
                            return false;
                        
                        bool matches = false;
                        foreach (var conditionName in conditionNames)
                        {
                            var trimmedConditionName = conditionName.Trim();
                            if (carConditionName.Equals(trimmedConditionName, StringComparison.OrdinalIgnoreCase))
                            {
                                matches = true;
                                break;
                            }
                        }
                        if (!matches)
                            return false;
                    }
                }
                
                // Проверка пробега
                else if (trimmedCondition.StartsWith("пробег<") || trimmedCondition.StartsWith("пробег <"))
                {
                    if (TryParseValue(trimmedCondition, "пробег", out float maxMileage))
                    {
                        if (car.Mileage == null || car.Mileage >= maxMileage)
                            return false;
                    }
                }
                else if (trimmedCondition.StartsWith("пробег>") || trimmedCondition.StartsWith("пробег >"))
                {
                    if (TryParseValue(trimmedCondition, "пробег", out float minMileage))
                    {
                        if (car.Mileage == null || car.Mileage <= minMileage)
                            return false;
                    }
                }
                
                // Проверка количества покупок клиента
                else if (trimmedCondition.StartsWith("покупок>") || trimmedCondition.StartsWith("покупок >"))
                {
                    if (TryParseValue(trimmedCondition, "покупок", out float minPurchases))
                    {
                        if (clientPurchaseCount <= minPurchases)
                            return false;
                    }
                }
                else if (trimmedCondition.StartsWith("покупок<") || trimmedCondition.StartsWith("покупок <"))
                {
                    if (TryParseValue(trimmedCondition, "покупок", out float maxPurchases))
                    {
                        if (clientPurchaseCount >= maxPurchases)
                            return false;
                    }
                }
                else if (trimmedCondition.StartsWith("покупок>=") || trimmedCondition.StartsWith("покупок >="))
                {
                    if (TryParseValue(trimmedCondition, "покупок", out float minPurchases))
                    {
                        if (clientPurchaseCount < minPurchases)
                            return false;
                    }
                }
                else if (trimmedCondition.StartsWith("покупок=") || trimmedCondition.StartsWith("покупок ="))
                {
                    if (TryParseValue(trimmedCondition, "покупок", out float exactPurchases))
                    {
                        if (clientPurchaseCount != exactPurchases)
                            return false;
                    }
                }
                
                // Проверка первого клиента (первая покупка)
                else if (trimmedCondition == "первый_клиент" || trimmedCondition == "первый клиент" || trimmedCondition == "первая покупка")
                {
                    // Если это условие для первого клиента, проверяем, что у клиента 0 покупок
                    if (clientPurchaseCount > 0)
                        return false;
                }
                
                // Проверка VIP клиента (3+ покупок)
                else if (trimmedCondition == "vip_клиент" || trimmedCondition == "vip клиент" || trimmedCondition == "вип клиент")
                {
                    // Если это условие для VIP клиента, проверяем, что у клиента 3 или больше покупок
                    if (clientPurchaseCount < 3)
                        return false;
                }
            }
            
            // Если все условия выполнены, скидка применима
            return true;
        }

        private bool TryParseValue(string condition, string prefix, out float value)
        {
            value = 0;
            // Ищем оператор сравнения
            int operatorIndex = -1;
            if (condition.Contains(">="))
                operatorIndex = condition.IndexOf(">=");
            else if (condition.Contains("<="))
                operatorIndex = condition.IndexOf("<=");
            else if (condition.Contains(">"))
                operatorIndex = condition.IndexOf(">");
            else if (condition.Contains("<"))
                operatorIndex = condition.IndexOf("<");
            else if (condition.Contains("="))
                operatorIndex = condition.IndexOf("=");
            
            if (operatorIndex >= 0 && operatorIndex < condition.Length - 1)
            {
                var valuePart = condition.Substring(operatorIndex + 1).Trim();
                // Убираем возможные пробелы и символы оператора
                valuePart = valuePart.TrimStart('=', '>', '<');
                if (float.TryParse(valuePart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float parsedValue))
                {
                    value = parsedValue;
                    return true;
                }
            }
            return false;
        }

        private string ExtractValue(string condition, string prefix)
        {
            var index = condition.IndexOf('=');
            if (index >= 0 && index < condition.Length - 1)
            {
                return condition.Substring(index + 1).Trim();
            }
            return string.Empty;
        }

        public event Action? DiscountsAutoApplied;
        private void OnDiscountsAutoApplied()
        {
            DiscountsAutoApplied?.Invoke();
        }

        partial void OnPriceChanged(string value)
        {
            if (!_isPageActive) return;
            UpdateFinalPriceCommand.ExecuteAsync(null);
        }

        partial void OnClientNameChanged(string value)
        {
            if (!_isPageActive) return;
            
            // Перепроверяем условия скидок при изменении имени клиента
            if (!string.IsNullOrWhiteSpace(value) && Discounts != null && Discounts.Any() && SelectedCar != null)
            {
                // Небольшая задержка, чтобы пользователь успел ввести полное имя
                Task.Delay(500).ContinueWith(async _ =>
                {
                    if (!_isPageActive) return;
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        if (!_isPageActive) return;
                        RecheckDiscountConditions();
                    });
                });
            }
        }

        partial void OnSelectedCarChanged(CarDisplayItem? value)
        {
            if (!_isPageActive) return;
            
            if (value != null)
            {
                Price = value.Car.Cost?.ToString("F0") ?? "0";
                
                // Обновляем информацию об автомобиле
                var car = value.Car;
                CarBrand = car.Model?.Brand?.Name ?? "—";
                CarModel = car.Model?.Name ?? "—";
                CarYear = car.Year?.ToString() ?? "—";
                CarColor = car.Color ?? "—";
                CarEngine = car.EngType?.Name ?? "—";
                CarEngineVolume = car.EngVol.HasValue ? $"{car.EngVol:F1} л" : "—";
                CarMileage = car.Mileage.HasValue ? $"{car.Mileage:N0} км" : "—";
                CarTransmission = car.Transmission?.Name ?? "—";
                CarType = car.Type?.Name ?? "—";
                CarDrive = car.Wd?.Name ?? "—";
                CarPower = car.Power.HasValue ? $"{car.Power} л.с." : "—";
                CarCondition = car.Condition?.Name ?? "—";
                IsCarInfoVisible = true;
                
                // Перепроверяем условия скидок при выборе автомобиля
                if (Discounts != null && Discounts.Any())
                {
                    RecheckDiscountConditions();
                }
            }
            else
            {
                // Если автомобиль не выбран, скрываем информацию и скидки
                IsCarInfoVisible = false;
                CarBrand = string.Empty;
                CarModel = string.Empty;
                CarYear = string.Empty;
                CarColor = string.Empty;
                CarEngine = string.Empty;
                CarEngineVolume = string.Empty;
                CarMileage = string.Empty;
                CarTransmission = string.Empty;
                CarType = string.Empty;
                CarDrive = string.Empty;
                CarPower = string.Empty;
                CarCondition = string.Empty;
                ApplicableDiscounts.Clear();
            }
        }

        private bool _isCheckingDiscounts = false;

        private async void RecheckDiscountConditions()
        {
            if (!_isPageActive || _isCheckingDiscounts) return;
            
            _isCheckingDiscounts = true;
            try
            {
                await CheckAndApplyDiscountsAsync();
            }
            finally
            {
                _isCheckingDiscounts = false;
            }
        }

        partial void OnSelectedClientChanged(Client? value)
        {
            if (!_isPageActive) return;
            
            if (value != null)
            {
                var fullName = $"{value.Surname} {value.Name} {value.Patronyc}".Trim();
                ClientName = fullName;
                ClientPhone = value.PhoneNumber ?? string.Empty;
                ClientPassData = value.PassData ?? string.Empty;
                IsClientsVisible = false;
                ShowAllClients = false;
                ClientSearchText = string.Empty;
                
                // Перепроверяем условия скидок при выборе клиента
                if (Discounts != null && Discounts.Any() && SelectedCar != null)
                {
                    RecheckDiscountConditions();
                }
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
                FoundClients = await _userService.SearchClientEntitiesAsync(searchText);
                if (!_isPageActive) return;
                
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
        private void SelectClient(Client? client)
        {
            // Логика выбора клиента обрабатывается в OnSelectedClientChanged
            SelectedClient = client;
        }

        [RelayCommand]
        private async Task FindClientAsync()
        {
            if (!_isPageActive) return;
            
            if (string.IsNullOrWhiteSpace(ClientName))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите ФИО клиента для поиска", "OK");
                return;
            }

            try
            {
                var client = await _userService.SearchClientEntityByNameAsync(ClientName);
                if (!_isPageActive) return;
                
                if (client != null)
                {
                    SelectedClient = client;
                    var fullName = $"{client.Surname} {client.Name} {client.Patronyc}".Trim();
                    ClientName = fullName;
                    ClientPhone = client.PhoneNumber ?? string.Empty;
                    ClientPassData = client.PassData ?? string.Empty;
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
            if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
            
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

            // Применяем скидку (только одну)
            var selectedDiscountIds = _selectedDiscount != null 
                ? new List<int> { _selectedDiscount.Id } 
                : new List<int>();

            try
            {
                // Проверяем, что страница все еще активна перед запросом к БД
                if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
                
                var finalPrice = await _saleService.CalculateFinalPriceAsync(priceWithAdditions, selectedDiscountIds);
                
                // Проверяем еще раз после запроса
                if (!_isPageActive || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;
                
                FinalPrice = $"Итоговая цена: {finalPrice:N0} ₽";
                if (additionsCost > 0)
                {
                    FinalPrice += $"\n(базовая: {basePrice:N0} ₽ + опции: {additionsCost:N0} ₽)";
                }
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмененные операции
            }
            catch
            {
                // Игнорируем другие ошибки, только если страница все еще активна
                if (_isPageActive)
                {
                    FinalPrice = "Итоговая цена: 0 ₽";
                }
            }
        }

        public void OnAdditionSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (!_isPageActive) return;
            
            _selectedAdditions = selectedItems.OfType<Addition>().ToList();
            UpdateFinalPriceCommand.ExecuteAsync(null);
        }

        public void OnDiscountSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (!_isPageActive) return;
            
            // При одиночном выборе используем SelectedItem из биндинга
            // Если SelectedDiscount изменился, обновляем цену
            _discountsAutoApplied = true;
            UpdateFinalPriceCommand.ExecuteAsync(null);
        }

        private void UpdateDiscountSelectionItems(List<Discount> discounts)
        {
            // Сохраняем текущий выбор
            var currentSelectedId = SelectedDiscount?.Id;
            
            // Обновляем список элементов выбора
            DiscountSelectionItems = discounts.Select(d => new DiscountSelectionItem 
            { 
                Discount = d, 
                IsSelected = d.Id == currentSelectedId 
            }).ToList();
        }

        public void OnDiscountCheckboxChanged(DiscountSelectionItem item)
        {
            if (!_isPageActive || item == null) return;
            
            if (item.IsSelected)
            {
                // Если выбрана скидка, снимаем выбор со всех остальных
                foreach (var discountItem in DiscountSelectionItems)
                {
                    if (discountItem != item)
                    {
                        discountItem.IsSelected = false;
                    }
                }
                SelectedDiscount = item.Discount;
            }
            else
            {
                // Если снят выбор, очищаем выбранную скидку
                SelectedDiscount = null;
            }
            
            _discountsAutoApplied = true;
            UpdateFinalPriceCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task CreateSaleAsync()
        {
            if (!_isPageActive) return;
            
            // Валидация
            if (SelectedCar == null || SelectedCar.Car == null)
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
                // Создаем или получаем Client entity для связи с продажей
                // Всегда используем CreateOrGetClientEntityAsync, чтобы убедиться, что данные сохранены
                var clientEntity = await _userService.CreateOrGetClientEntityAsync(
                    ClientName.Trim(),
                    ClientPhone?.Trim(),
                    ClientPassData?.Trim());

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
                var selectedDiscountIds = _selectedDiscount != null 
                    ? new List<int> { _selectedDiscount.Id } 
                    : new List<int>();
                var finalPrice = await _saleService.CalculateFinalPriceAsync(priceWithAdditions, selectedDiscountIds);

                // Проверяем, что все данные корректны (проверки уже были выше, но убеждаемся что объекты загружены)
                if (SelectedCar?.Car == null)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не выбран автомобиль", "OK");
                    return;
                }

                if (SelectedManager?.Manager == null)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не выбран менеджер", "OK");
                    return;
                }

                // Проверяем, что ID автомобиля валиден
                var carId = SelectedCar.Car.Id;
                var managerId = SelectedManager.Manager.Id;
                
                // Создаем продажу
                var sale = new Sale
                {
                    CarId = carId,
                    ManagerId = managerId,
                    ClientId = clientEntity.Id,
                    Date = DateOnly.FromDateTime(SaleDate),
                    Cost = (float)finalPrice
                };

                var selectedAdditionIds = _selectedAdditions.Select(a => a.Id).ToList();

                // Сохраняем продажу
                var createdSale = await _saleService.CreateSaleAsync(sale, selectedAdditionIds, selectedDiscountIds);

                // Разблокируем навигацию после успешного создания
                IsNavigationLocked = false;
                MessagingCenter.Send(this, "NavigationLocked", false);
                
                // Открываем страницу предпросмотра договора
                await Shell.Current.GoToAsync($"ContractPreviewPage?saleId={createdSale.Id}&basePrice={price}&finalPrice={finalPrice}");
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
            // Устанавливаем флаг, что отмена вызвана явно
            IsCancelling = true;
            
            try
            {
                // Отменяем все активные операции
                ClearData();
                
                // Разблокируем навигацию после отмены
                IsNavigationLocked = false;
                MessagingCenter.Send(this, "NavigationLocked", false);
                
                // Возвращаемся на предыдущую страницу
                await Shell.Current.GoToAsync("..");
            }
            catch
            {
                // Если навигация не удалась, все равно пытаемся вернуться
                try
                {
                    IsNavigationLocked = false;
                    MessagingCenter.Send(this, "NavigationLocked", false);
                    await Shell.Current.GoToAsync("..");
                }
                catch
                {
                    // Игнорируем ошибки навигации
                }
            }
            finally
            {
                // Сбрасываем флаг после завершения
                IsCancelling = false;
            }
        }

        private async Task GenerateContractAsync(Sale sale, Client client, decimal finalPrice, decimal basePrice)
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

    public class DiscountSelectionItem : ObservableObject
    {
        public Discount Discount { get; set; } = null!;
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}

