using CarShowroom.Services;
using DataLayer.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CarShowroom
{
    public partial class AddEditCarPage : ContentPage
    {
        private CarService? _carService;
        private readonly long? _carId;
        private bool _isEditMode => _carId.HasValue;
        private List<Brand> _brands = new();
        private List<Model> _models = new();
        private List<CarType> _carTypes = new();
        private List<ConditionType> _conditionTypes = new();
        private List<EngineType> _engineTypes = new();
        private List<Transmission> _transmissions = new();
        private List<Wdtype> _wdTypes = new();

        public AddEditCarPage(long? carId = null)
        {
            InitializeComponent();
            _carId = carId;

            if (_isEditMode)
            {
                Title = "Редактировать автомобиль";
            }
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
            
            while (_carService == null && attempt < maxAttempts)
            {
                if (Handler?.MauiContext?.Services != null)
                {
                    _carService = Handler.MauiContext.Services.GetService<CarService>();
                    if (_carService != null)
                    {
                        await LoadDataAsync();
                        return;
                    }
                }
                
                await Task.Delay(50);
                attempt++;
            }
            
            if (_carService == null)
            {
                await DisplayAlert("Ошибка", "CarService не найден", "OK");
                await Navigation.PopAsync();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Если сервис еще не получен, пытаемся получить
            if (_carService == null)
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
            if (_carService == null)
            {
                await DisplayAlert("Ошибка", "CarService не инициализирован", "OK");
                return;
            }

            try
            {
                // Загружаем справочники
                _brands = await _carService.GetAllBrandsAsync();
                _carTypes = await _carService.GetAllCarTypesAsync();
                _conditionTypes = await _carService.GetAllConditionTypesAsync();
                _engineTypes = await _carService.GetAllEngineTypesAsync();
                _transmissions = await _carService.GetAllTransmissionsAsync();
                _wdTypes = await _carService.GetAllWdTypesAsync();

                // Заполняем Picker'ы
                BrandPicker.ItemsSource = _brands;
                BrandPicker.ItemDisplayBinding = new Binding("Name");
                BrandPicker.SelectedIndexChanged += OnBrandChanged;

                CarTypePicker.ItemsSource = _carTypes;
                CarTypePicker.ItemDisplayBinding = new Binding("Name");

                ConditionPicker.ItemsSource = _conditionTypes;
                ConditionPicker.ItemDisplayBinding = new Binding("Name");

                EngineTypePicker.ItemsSource = _engineTypes;
                EngineTypePicker.ItemDisplayBinding = new Binding("Name");

                TransmissionPicker.ItemsSource = _transmissions;
                TransmissionPicker.ItemDisplayBinding = new Binding("Name");

                WdTypePicker.ItemsSource = _wdTypes;
                WdTypePicker.ItemDisplayBinding = new Binding("Name");

                // Обработчик для Switch состояния "В наличии"
                StockSwitch.Toggled += OnStockSwitchToggled;
                UpdateStockLabel();

                if (_isEditMode && _carId.HasValue)
                {
                    await LoadCarDataAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
        }

        private async void OnBrandChanged(object? sender, EventArgs e)
        {
            if (_carService == null) return;
            
            if (BrandPicker.SelectedItem is Brand selectedBrand)
            {
                _models = await _carService.GetModelsByBrandIdAsync(selectedBrand.Id);
                ModelPicker.ItemsSource = _models;
                ModelPicker.ItemDisplayBinding = new Binding("Name");
            }
        }

        private void OnStockSwitchToggled(object? sender, ToggledEventArgs e)
        {
            UpdateStockLabel();
        }

        private void UpdateStockLabel()
        {
            StockLabel.Text = StockSwitch.IsToggled ? "В наличии" : "Нет в наличии";
        }

        private async Task LoadCarDataAsync()
        {
            if (_carService == null)
            {
                await DisplayAlert("Ошибка", "CarService не инициализирован", "OK");
                return;
            }

            if (_carId.HasValue)
            {
                var car = await _carService.GetCarByIdAsync(_carId.Value);
                if (car != null)
                {
                    // Заполняем основные поля
                    YearEntry.Text = car.Year?.ToString() ?? string.Empty;
                    ColorEntry.Text = car.Color ?? string.Empty;
                    PriceEntry.Text = car.Cost?.ToString("F0") ?? string.Empty;
                    EngineVolumeEntry.Text = car.EngVol?.ToString() ?? string.Empty;
                    PowerEntry.Text = car.Power?.ToString() ?? string.Empty;
                    MileageEntry.Text = car.Mileage?.ToString() ?? string.Empty;
                    StockSwitch.IsToggled = car.Stock ?? false;

                    // Выбираем в Picker'ах
                    if (car.Model?.BrandId.HasValue == true)
                    {
                        var brand = _brands.FirstOrDefault(b => b.Id == car.Model.BrandId.Value);
                        if (brand != null)
                        {
                            BrandPicker.SelectedItem = brand;
                            await Task.Delay(100); // Небольшая задержка для загрузки моделей
                            if (car.ModelId.HasValue)
                            {
                                var model = _models.FirstOrDefault(m => m.Id == car.ModelId.Value);
                                if (model != null)
                                {
                                    ModelPicker.SelectedItem = model;
                                }
                            }
                        }
                    }

                    if (car.TypeId.HasValue)
                    {
                        var carType = _carTypes.FirstOrDefault(t => t.Id == car.TypeId.Value);
                        if (carType != null) CarTypePicker.SelectedItem = carType;
                    }

                    if (car.ConditionId.HasValue)
                    {
                        var condition = _conditionTypes.FirstOrDefault(c => c.Id == car.ConditionId.Value);
                        if (condition != null) ConditionPicker.SelectedItem = condition;
                    }

                    if (car.EngTypeId.HasValue)
                    {
                        var engType = _engineTypes.FirstOrDefault(e => e.Id == car.EngTypeId.Value);
                        if (engType != null) EngineTypePicker.SelectedItem = engType;
                    }

                    if (car.TransmissionId.HasValue)
                    {
                        var transmission = _transmissions.FirstOrDefault(t => t.Id == car.TransmissionId.Value);
                        if (transmission != null) TransmissionPicker.SelectedItem = transmission;
                    }

                    if (car.WdId.HasValue)
                    {
                        var wdType = _wdTypes.FirstOrDefault(w => w.Id == car.WdId.Value);
                        if (wdType != null) WdTypePicker.SelectedItem = wdType;
                    }
                }
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Валидация
            if (BrandPicker.SelectedItem == null ||
                ModelPicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(YearEntry.Text) ||
                string.IsNullOrWhiteSpace(ColorEntry.Text) ||
                string.IsNullOrWhiteSpace(PriceEntry.Text) ||
                EngineTypePicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(EngineVolumeEntry.Text) ||
                string.IsNullOrWhiteSpace(MileageEntry.Text) ||
                TransmissionPicker.SelectedItem == null ||
                CarTypePicker.SelectedItem == null ||
                ConditionPicker.SelectedItem == null ||
                WdTypePicker.SelectedItem == null)
            {
                await DisplayAlert("Ошибка", "Пожалуйста, заполните все обязательные поля", "OK");
                return;
            }

            if (!int.TryParse(YearEntry.Text, out int year) || year < 1900 || year > DateTime.Now.Year + 1)
            {
                await DisplayAlert("Ошибка", "Введите корректный год", "OK");
                return;
            }

            if (!float.TryParse(PriceEntry.Text, out float price) || price <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректную цену", "OK");
                return;
            }

            if (!float.TryParse(EngineVolumeEntry.Text, out float engineVolume) || engineVolume <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректный объем двигателя", "OK");
                return;
            }

            if (!float.TryParse(MileageEntry.Text, out float mileage) || mileage < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректный пробег", "OK");
                return;
            }

            float? power = null;
            if (!string.IsNullOrWhiteSpace(PowerEntry.Text))
            {
                if (float.TryParse(PowerEntry.Text, out float powerValue))
                {
                    power = powerValue;
                }
            }

            // Получаем выбранные объекты
            var selectedBrand = (Brand)BrandPicker.SelectedItem;
            var selectedModel = (Model)ModelPicker.SelectedItem;
            var selectedCarType = (CarType)CarTypePicker.SelectedItem;
            var selectedCondition = (ConditionType)ConditionPicker.SelectedItem;
            var selectedEngineType = (EngineType)EngineTypePicker.SelectedItem;
            var selectedTransmission = (Transmission)TransmissionPicker.SelectedItem;
            var selectedWdType = (Wdtype)WdTypePicker.SelectedItem;

            var car = new Car
            {
                Year = year,
                Color = ColorEntry.Text.Trim(),
                Cost = price,
                EngVol = engineVolume,
                Power = power,
                Mileage = mileage,
                Stock = StockSwitch.IsToggled,
                ModelId = selectedModel.Id,
                TypeId = selectedCarType.Id,
                ConditionId = selectedCondition.Id,
                EngTypeId = selectedEngineType.Id,
                TransmissionId = selectedTransmission.Id,
                WdId = selectedWdType.Id
            };

            if (_carService == null)
            {
                await DisplayAlert("Ошибка", "CarService не инициализирован", "OK");
                return;
            }

            try
            {
                if (_isEditMode && _carId.HasValue)
                {
                    car.Id = _carId.Value;
                    await _carService.UpdateCarAsync(car);
                    await DisplayAlert("Успех", "Автомобиль обновлен", "OK");
                }
                else
                {
                    await _carService.AddCarAsync(car);
                    await DisplayAlert("Успех", "Автомобиль добавлен", "OK");
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить автомобиль: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
