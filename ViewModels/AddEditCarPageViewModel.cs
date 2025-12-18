using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using DataLayer.Entities;

namespace CarShowroom.ViewModels
{
    public partial class AddEditCarPageViewModel : ObservableObject
    {
        private readonly ICarService _carService;

        [ObservableProperty]
        private List<Brand> _brands = new();

        [ObservableProperty]
        private List<Model> _models = new();

        [ObservableProperty]
        private List<CarType> _carTypes = new();

        [ObservableProperty]
        private List<ConditionType> _conditionTypes = new();

        [ObservableProperty]
        private List<EngineType> _engineTypes = new();

        [ObservableProperty]
        private List<Transmission> _transmissions = new();

        [ObservableProperty]
        private List<Wdtype> _wdTypes = new();

        [ObservableProperty]
        private Brand? _selectedBrand;

        [ObservableProperty]
        private Model? _selectedModel;

        [ObservableProperty]
        private CarType? _selectedCarType;

        [ObservableProperty]
        private ConditionType? _selectedCondition;

        [ObservableProperty]
        private EngineType? _selectedEngineType;

        [ObservableProperty]
        private Transmission? _selectedTransmission;

        [ObservableProperty]
        private Wdtype? _selectedWdType;

        [ObservableProperty]
        private string _year = string.Empty;

        [ObservableProperty]
        private string _color = string.Empty;

        [ObservableProperty]
        private string _price = string.Empty;

        [ObservableProperty]
        private string _engineVolume = string.Empty;

        [ObservableProperty]
        private string _power = string.Empty;

        [ObservableProperty]
        private string _mileage = string.Empty;

        [ObservableProperty]
        private bool _isInStock = true;

        [ObservableProperty]
        private string _stockLabel = "В наличии";

        [ObservableProperty]
        private string _title = "Добавить автомобиль";

        private long? _carId;
        private bool _isEditMode => _carId.HasValue;

        public AddEditCarPageViewModel(ICarService carService)
        {
            _carService = carService;
        }

        public void Initialize(long? carId = null)
        {
            _carId = carId;
            if (_isEditMode)
            {
                Title = "Редактировать автомобиль";
            }
            LoadDataCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                Brands = await _carService.GetAllBrandsAsync();
                CarTypes = await _carService.GetAllCarTypesAsync();
                ConditionTypes = await _carService.GetAllConditionTypesAsync();
                EngineTypes = await _carService.GetAllEngineTypesAsync();
                Transmissions = await _carService.GetAllTransmissionsAsync();
                WdTypes = await _carService.GetAllWdTypesAsync();

                if (_isEditMode && _carId.HasValue)
                {
                    await LoadCarDataAsync();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        partial void OnSelectedBrandChanged(Brand? value)
        {
            if (value != null)
            {
                LoadModelsCommand.ExecuteAsync(null);
            }
        }

        partial void OnIsInStockChanged(bool value)
        {
            StockLabel = value ? "В наличии" : "Нет в наличии";
        }

        [RelayCommand]
        private async Task LoadModelsAsync()
        {
            if (SelectedBrand != null)
            {
                Models = await _carService.GetModelsByBrandIdAsync(SelectedBrand.Id);
            }
        }

        private async Task LoadCarDataAsync()
        {
            if (!_carId.HasValue) return;

            var car = await _carService.GetCarByIdAsync(_carId.Value);
            if (car != null)
            {
                Year = car.Year?.ToString() ?? string.Empty;
                Color = car.Color ?? string.Empty;
                Price = car.Cost?.ToString("F0") ?? string.Empty;
                EngineVolume = car.EngVol?.ToString() ?? string.Empty;
                Power = car.Power?.ToString() ?? string.Empty;
                Mileage = car.Mileage?.ToString() ?? string.Empty;
                IsInStock = car.Stock ?? false;

                if (car.Model?.BrandId.HasValue == true)
                {
                    SelectedBrand = Brands.FirstOrDefault(b => b.Id == car.Model.BrandId.Value);
                    if (SelectedBrand != null)
                    {
                        await LoadModelsAsync();
                        if (car.ModelId.HasValue)
                        {
                            SelectedModel = Models.FirstOrDefault(m => m.Id == car.ModelId.Value);
                        }
                    }
                }

                if (car.TypeId.HasValue)
                    SelectedCarType = CarTypes.FirstOrDefault(t => t.Id == car.TypeId.Value);

                if (car.ConditionId.HasValue)
                    SelectedCondition = ConditionTypes.FirstOrDefault(c => c.Id == car.ConditionId.Value);

                if (car.EngTypeId.HasValue)
                    SelectedEngineType = EngineTypes.FirstOrDefault(e => e.Id == car.EngTypeId.Value);

                if (car.TransmissionId.HasValue)
                    SelectedTransmission = Transmissions.FirstOrDefault(t => t.Id == car.TransmissionId.Value);

                if (car.WdId.HasValue)
                    SelectedWdType = WdTypes.FirstOrDefault(w => w.Id == car.WdId.Value);
            }
        }

        [RelayCommand]
        private async Task SaveCarAsync()
        {
            // Валидация
            if (SelectedBrand == null ||
                SelectedModel == null ||
                string.IsNullOrWhiteSpace(Year) ||
                string.IsNullOrWhiteSpace(Color) ||
                string.IsNullOrWhiteSpace(Price) ||
                SelectedEngineType == null ||
                string.IsNullOrWhiteSpace(EngineVolume) ||
                string.IsNullOrWhiteSpace(Mileage) ||
                SelectedTransmission == null ||
                SelectedCarType == null ||
                SelectedCondition == null ||
                SelectedWdType == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, заполните все обязательные поля", "OK");
                return;
            }

            if (!int.TryParse(Year, out int year) || year < 1900 || year > DateTime.Now.Year + 1)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите корректный год", "OK");
                return;
            }

            if (!float.TryParse(Price, out float price) || price <= 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите корректную цену", "OK");
                return;
            }

            if (!float.TryParse(EngineVolume, out float engineVolume) || engineVolume <= 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите корректный объем двигателя", "OK");
                return;
            }

            if (!float.TryParse(Mileage, out float mileage) || mileage < 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите корректный пробег", "OK");
                return;
            }

            float? power = null;
            if (!string.IsNullOrWhiteSpace(Power))
            {
                if (float.TryParse(Power, out float powerValue))
                {
                    power = powerValue;
                }
            }

            var car = new Car
            {
                Year = year,
                Color = Color.Trim(),
                Cost = price,
                EngVol = engineVolume,
                Power = power,
                Mileage = mileage,
                Stock = IsInStock,
                ModelId = SelectedModel.Id,
                TypeId = SelectedCarType.Id,
                ConditionId = SelectedCondition.Id,
                EngTypeId = SelectedEngineType.Id,
                TransmissionId = SelectedTransmission.Id,
                WdId = SelectedWdType.Id
            };

            try
            {
                if (_isEditMode && _carId.HasValue)
                {
                    car.Id = _carId.Value;
                    await _carService.UpdateCarAsync(car);
                    await Shell.Current.DisplayAlert("Успех", "Автомобиль обновлен", "OK");
                }
                else
                {
                    await _carService.AddCarAsync(car);
                    await Shell.Current.DisplayAlert("Успех", "Автомобиль добавлен", "OK");
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить автомобиль: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}

