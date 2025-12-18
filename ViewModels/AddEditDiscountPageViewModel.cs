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
        private Brand? _selectedBrand;

        [ObservableProperty]
        private List<CarType> _carTypes = new();

        [ObservableProperty]
        private CarType? _selectedCarType;

        [ObservableProperty]
        private List<ConditionType> _conditionTypes = new();

        [ObservableProperty]
        private ConditionType? _selectedCondition;

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
                
                // Парсим описание и заполняем поля
                ParseDescription(discount.Description ?? string.Empty);
            }
        }

        private void ParseDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return;

            // Парсим условия, сохраняя оригинальный регистр для значений
            var conditions = description.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
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
                
                // Тип
                else if (trimmedLower.StartsWith("тип=") || trimmedLower.StartsWith("тип ="))
                {
                    var typeName = ExtractValue(trimmed);
                    SelectedCarType = CarTypes.FirstOrDefault(t => t.Name != null && t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                }
                
                // Бренд
                else if (trimmedLower.StartsWith("бренд=") || trimmedLower.StartsWith("бренд ="))
                {
                    var brandName = ExtractValue(trimmed);
                    if (!string.IsNullOrWhiteSpace(brandName))
                    {
                        SelectedBrand = Brands.FirstOrDefault(b => b.Name != null && b.Name.Equals(brandName, StringComparison.OrdinalIgnoreCase));
                    }
                }
                
                // Состояние
                else if (trimmedLower.StartsWith("состояние=") || trimmedLower.StartsWith("состояние ="))
                {
                    var conditionName = ExtractValue(trimmed);
                    SelectedCondition = ConditionTypes.FirstOrDefault(c => c.Name != null && c.Name.Equals(conditionName, StringComparison.OrdinalIgnoreCase));
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
            
            // Тип
            if (SelectedCarType != null && !string.IsNullOrWhiteSpace(SelectedCarType.Name))
                conditions.Add($"тип={SelectedCarType.Name}");
            
            // Бренд
            if (SelectedBrand != null && !string.IsNullOrWhiteSpace(SelectedBrand.Name))
                conditions.Add($"бренд={SelectedBrand.Name}");
            
            // Состояние
            if (SelectedCondition != null && !string.IsNullOrWhiteSpace(SelectedCondition.Name))
                conditions.Add($"состояние={SelectedCondition.Name}");
            
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
            
            return string.Join(", ", conditions);
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

            // Формируем описание из заполненных полей
            var description = BuildDescription();
            
            var discount = new Discount
            {
                Name = Name.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                Cost = cost
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

}
