using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using DataLayer.Entities;

namespace CarShowroom.ViewModels
{
    public partial class AddEditDiscountPageViewModel : ObservableObject
    {
        private readonly ISaleService _saleService;
        private readonly ICarService _carService;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _cost = string.Empty;

        // Условия по автомобилю
        [ObservableProperty]
        private string _minPrice = string.Empty;

        [ObservableProperty]
        private string _maxPrice = string.Empty;

        [ObservableProperty]
        private string _minYear = string.Empty;

        [ObservableProperty]
        private string _maxYear = string.Empty;

        [ObservableProperty]
        private List<Brand> _brands = new();

        [ObservableProperty]
        private List<BrandSelectionItem> _brandSelectionItems = new();

        [ObservableProperty]
        private List<CarType> _carTypes = new();

        [ObservableProperty]
        private List<CarTypeSelectionItem> _carTypeSelectionItems = new();

        [ObservableProperty]
        private List<ConditionType> _conditionTypes = new();

        [ObservableProperty]
        private List<ConditionTypeSelectionItem> _conditionTypeSelectionItems = new();

        [ObservableProperty]
        private string _minMileage = string.Empty;

        [ObservableProperty]
        private string _maxMileage = string.Empty;

        // Условия по клиенту
        [ObservableProperty]
        private bool _isFirstClient = false;

        [ObservableProperty]
        private bool _isVipClient = false;

        [ObservableProperty]
        private string _minPurchases = string.Empty;

        [ObservableProperty]
        private string _maxPurchases = string.Empty;

        // Срок действия акции
        [ObservableProperty]
        private DateTime _startDate = DateTime.Now;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now.AddMonths(1);

        [ObservableProperty]
        private bool _hasDateRange = false;

        [ObservableProperty]
        private string _title = "Добавить акцию";

        private int? _discountId;
        private bool _isEditMode => _discountId.HasValue;

        public AddEditDiscountPageViewModel(ISaleService saleService, ICarService carService)
        {
            _saleService = saleService;
            _carService = carService;
        }

        public void Initialize(int? discountId = null)
        {
            _discountId = discountId;
            if (_isEditMode)
            {
                Title = "Редактировать акцию";
            }
            LoadDataCommand.ExecuteAsync(null);
        }


        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                // Загружаем справочники
                Brands = await _carService.GetAllBrandsAsync();
                CarTypes = await _carService.GetAllCarTypesAsync();
                ConditionTypes = await _carService.GetAllConditionTypesAsync();

                // Инициализируем списки для множественного выбора
                BrandSelectionItems = Brands.Select(b => new BrandSelectionItem { Brand = b, IsSelected = false }).ToList();
                CarTypeSelectionItems = CarTypes.Select(t => new CarTypeSelectionItem { CarType = t, IsSelected = false }).ToList();
                ConditionTypeSelectionItems = ConditionTypes.Select(c => new ConditionTypeSelectionItem { ConditionType = c, IsSelected = false }).ToList();

                if (_isEditMode && _discountId.HasValue)
                {
                    await LoadDiscountDataAsync();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        private async Task LoadDiscountDataAsync()
        {
            if (!_discountId.HasValue) return;

            var discount = await _saleService.GetDiscountByIdAsync(_discountId.Value);
            if (discount != null)
            {
                Name = discount.Name ?? string.Empty;
                Cost = discount.Cost?.ToString("F2") ?? string.Empty;
                
                // Загружаем даты
                if (discount.StartDate.HasValue)
                {
                    StartDate = discount.StartDate.Value.ToDateTime(TimeOnly.MinValue);
                    HasDateRange = true;
                }
                if (discount.EndDate.HasValue)
                {
                    EndDate = discount.EndDate.Value.ToDateTime(TimeOnly.MinValue);
                    HasDateRange = true;
                }
                
                // Парсим описание и заполняем поля
                ParseDescription(discount.Description ?? string.Empty);
            }
        }

        private void ParseDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return;

            // Парсим условия, сохраняя оригинальный регистр для значений
            // Используем точку с запятой и перенос строки для разделения основных условий (новый формат)
            // Если точка с запятой не найдена, используем запятую для обратной совместимости (старый формат)
            var conditions = description.Contains(';') 
                ? description.Split(new[] { ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                : description.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var condition in conditions)
            {
                var trimmed = condition.Trim();
                var trimmedLower = trimmed.ToLower();
                
                // Цена
                if (trimmedLower.StartsWith("цена>") || trimmedLower.StartsWith("цена >"))
                {
                    if (TryParseValue(trimmed, out float value))
                        MinPrice = value.ToString("F0");
                }
                else if (trimmedLower.StartsWith("цена<") || trimmedLower.StartsWith("цена <"))
                {
                    if (TryParseValue(trimmed, out float value))
                        MaxPrice = value.ToString("F0");
                }
                
                // Год
                else if (trimmedLower.StartsWith("год>") || trimmedLower.StartsWith("год >"))
                {
                    if (TryParseValue(trimmed, out float value))
                        MinYear = value.ToString("F0");
                }
                else if (trimmedLower.StartsWith("год<") || trimmedLower.StartsWith("год <"))
                {
                    if (TryParseValue(trimmed, out float value))
                        MaxYear = value.ToString("F0");
                }
                
                // Тип (поддерживаем множественные значения через запятую)
                else if (trimmedLower.StartsWith("тип=") || trimmedLower.StartsWith("тип ="))
                {
                    var typeValue = ExtractValue(trimmed);
                    var typeNames = typeValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var typeName in typeNames)
                    {
                        var trimmedTypeName = typeName.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmedTypeName))
                        {
                            var item = CarTypeSelectionItems.FirstOrDefault(t => t.CarType.Name != null && t.CarType.Name.Equals(trimmedTypeName, StringComparison.OrdinalIgnoreCase));
                            if (item != null)
                                item.IsSelected = true;
                        }
                    }
                }
                
                // Бренд (поддерживаем множественные значения через запятую)
                else if (trimmedLower.StartsWith("бренд=") || trimmedLower.StartsWith("бренд ="))
                {
                    var brandValue = ExtractValue(trimmed);
                    var brandNames = brandValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var brandName in brandNames)
                    {
                        var trimmedBrandName = brandName.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmedBrandName))
                        {
                            var item = BrandSelectionItems.FirstOrDefault(b => b.Brand.Name != null && b.Brand.Name.Equals(trimmedBrandName, StringComparison.OrdinalIgnoreCase));
                            if (item != null)
                                item.IsSelected = true;
                        }
                    }
                }
                
                // Состояние (поддерживаем множественные значения через запятую)
                else if (trimmedLower.StartsWith("состояние=") || trimmedLower.StartsWith("состояние ="))
                {
                    var conditionValue = ExtractValue(trimmed);
                    var conditionNames = conditionValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var conditionName in conditionNames)
                    {
                        var trimmedConditionName = conditionName.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmedConditionName))
                        {
                            var item = ConditionTypeSelectionItems.FirstOrDefault(c => c.ConditionType.Name != null && c.ConditionType.Name.Equals(trimmedConditionName, StringComparison.OrdinalIgnoreCase));
                            if (item != null)
                                item.IsSelected = true;
                        }
                    }
                }
                
                // Пробег
                else if (trimmedLower.StartsWith("пробег>") || trimmedLower.StartsWith("пробег >"))
                {
                    if (TryParseValue(trimmed, out float value))
                        MinMileage = value.ToString("F0");
                }
                else if (trimmedLower.StartsWith("пробег<") || trimmedLower.StartsWith("пробег <"))
                {
                    if (TryParseValue(trimmed, out float value))
                        MaxMileage = value.ToString("F0");
                }
                
                // Клиент
                else if (trimmedLower.Contains("первый_клиент") || trimmedLower.Contains("первый клиент") || trimmedLower.Contains("первая покупка"))
                {
                    IsFirstClient = true;
                }
                else if (trimmedLower.Contains("vip_клиент") || trimmedLower.Contains("vip клиент") || trimmedLower.Contains("вип клиент"))
                {
                    IsVipClient = true;
                }
                
                // Покупки
                else if (trimmedLower.StartsWith("покупок>=") || trimmedLower.StartsWith("покупок >="))
                {
                    if (TryParseValue(trimmed, out float value))
                        MinPurchases = value.ToString("F0");
                }
                else if (trimmedLower.StartsWith("покупок<") || trimmedLower.StartsWith("покупок <"))
                {
                    if (TryParseValue(trimmed, out float value))
                        MaxPurchases = value.ToString("F0");
                }
            }
        }

        private bool TryParseValue(string condition, out float value)
        {
            value = 0;
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
                valuePart = valuePart.TrimStart('=', '>', '<');
                if (float.TryParse(valuePart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float parsedValue))
                {
                    value = parsedValue;
                    return true;
                }
            }
            return false;
        }

        private string ExtractValue(string condition)
        {
            var index = condition.IndexOf('=');
            if (index >= 0 && index < condition.Length - 1)
            {
                return condition.Substring(index + 1).Trim();
            }
            return string.Empty;
        }

        private string BuildDescription()
        {
            var conditions = new List<string>();
            
            // Цена
            if (!string.IsNullOrWhiteSpace(MinPrice) && float.TryParse(MinPrice, out float minPrice))
                conditions.Add($"цена>{minPrice:F0}");
            if (!string.IsNullOrWhiteSpace(MaxPrice) && float.TryParse(MaxPrice, out float maxPrice))
                conditions.Add($"цена<{maxPrice:F0}");
            
            // Год
            if (!string.IsNullOrWhiteSpace(MinYear) && float.TryParse(MinYear, out float minYear))
                conditions.Add($"год>{minYear:F0}");
            if (!string.IsNullOrWhiteSpace(MaxYear) && float.TryParse(MaxYear, out float maxYear))
                conditions.Add($"год<{maxYear:F0}");
            
            // Тип (множественный выбор)
            var selectedTypes = CarTypeSelectionItems.Where(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.CarType.Name)).Select(t => t.CarType.Name).ToList();
            if (selectedTypes.Any())
                conditions.Add($"тип={string.Join(",", selectedTypes)}");
            
            // Бренд (множественный выбор)
            var selectedBrands = BrandSelectionItems.Where(b => b.IsSelected && !string.IsNullOrWhiteSpace(b.Brand.Name)).Select(b => b.Brand.Name).ToList();
            if (selectedBrands.Any())
                conditions.Add($"бренд={string.Join(",", selectedBrands)}");
            
            // Состояние (множественный выбор)
            var selectedConditions = ConditionTypeSelectionItems.Where(c => c.IsSelected && !string.IsNullOrWhiteSpace(c.ConditionType.Name)).Select(c => c.ConditionType.Name).ToList();
            if (selectedConditions.Any())
                conditions.Add($"состояние={string.Join(",", selectedConditions)}");
            
            // Пробег
            if (!string.IsNullOrWhiteSpace(MinMileage) && float.TryParse(MinMileage, out float minMileage))
                conditions.Add($"пробег>{minMileage:F0}");
            if (!string.IsNullOrWhiteSpace(MaxMileage) && float.TryParse(MaxMileage, out float maxMileage))
                conditions.Add($"пробег<{maxMileage:F0}");
            
            // Клиент
            if (IsFirstClient)
                conditions.Add("первый_клиент");
            if (IsVipClient)
                conditions.Add("vip_клиент");
            
            // Покупки
            if (!string.IsNullOrWhiteSpace(MinPurchases) && float.TryParse(MinPurchases, out float minPurchases))
                conditions.Add($"покупок>={minPurchases:F0}");
            if (!string.IsNullOrWhiteSpace(MaxPurchases) && float.TryParse(MaxPurchases, out float maxPurchases))
                conditions.Add($"покупок<{maxPurchases:F0}");
            
            // Используем точку с запятой для разделения основных условий, чтобы запятая могла использоваться для множественных значений внутри условий
            return string.Join("; ", conditions);
        }

        [RelayCommand]
        private async Task SaveDiscountAsync()
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите название акции", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Cost))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите размер скидки", "OK");
                return;
            }

            if (!float.TryParse(Cost, out float cost) || cost < 0 || cost > 100)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите корректный размер скидки (от 0 до 100%)", "OK");
                return;
            }

            // Валидация дат
            if (HasDateRange && EndDate < StartDate)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Дата окончания не может быть раньше даты начала", "OK");
                return;
            }

            // Формируем описание из заполненных полей
            var description = BuildDescription();
            
            var discount = new Discount
            {
                Name = Name.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                Cost = cost,
                StartDate = HasDateRange ? DateOnly.FromDateTime(StartDate) : null,
                EndDate = HasDateRange ? DateOnly.FromDateTime(EndDate) : null
            };

            try
            {
                if (_isEditMode && _discountId.HasValue)
                {
                    discount.Id = _discountId.Value;
                    await _saleService.UpdateDiscountAsync(discount);
                    await Shell.Current.DisplayAlert("Успех", "Акция обновлена", "OK");
                }
                else
                {
                    await _saleService.CreateDiscountAsync(discount);
                    await Shell.Current.DisplayAlert("Успех", "Акция создана", "OK");
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить акцию: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    public class BrandSelectionItem : ObservableObject
    {
        public Brand Brand { get; set; } = null!;
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class CarTypeSelectionItem : ObservableObject
    {
        public CarType CarType { get; set; } = null!;
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class ConditionTypeSelectionItem : ObservableObject
    {
        public ConditionType ConditionType { get; set; } = null!;
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

}
