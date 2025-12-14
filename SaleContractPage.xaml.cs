namespace CarShowroom
{
    public partial class SaleContractPage : ContentPage
    {
        private int? _carId;
        private int? _contractId;

        public SaleContractPage(int? carId = null, int? contractId = null)
        {
            InitializeComponent();
            _contractService = new SaleContractService();
            _carService = new CarService();
            _carId = carId;
            _contractId = contractId;

            if (_contractId.HasValue)
            {
                Title = "Редактировать договор";
                LoadContractData();
            }
            else if (_carId.HasValue)
            {
                LoadCarData();
            }

            ContractDatePicker.Date = DateTime.Now;
        }

        private void LoadCarData()
        {
            if (_carId.HasValue)
            {
                var car = _carService.GetCarById(_carId.Value);
                if (car != null)
                {
                    CarBrandEntry.Text = car.Brand;
                    CarModelEntry.Text = car.Model;
                    CarYearEntry.Text = car.Year.ToString();
                    CarColorEntry.Text = car.Color;
                    CarVinEntry.Text = car.Vin;
                    CarEngineNumberEntry.Text = car.EngineNumber;
                    CarBodyNumberEntry.Text = car.BodyNumber;
                    CarRegistrationNumberEntry.Text = car.RegistrationNumber;
                    SalePriceEntry.Text = car.Price.ToString();
                }
            }
        }

        private void LoadContractData()
        {
            if (_contractId.HasValue)
            {
                var contract = _contractService.GetContractById(_contractId.Value);
                if (contract != null)
                {
                    ContractDatePicker.Date = contract.ContractDate;
                    CarBrandEntry.Text = contract.CarBrand;
                    CarModelEntry.Text = contract.CarModel;
                    CarYearEntry.Text = contract.CarYear.ToString();
                    CarVinEntry.Text = contract.CarVin;
                    CarEngineNumberEntry.Text = contract.CarEngineNumber;
                    CarBodyNumberEntry.Text = contract.CarBodyNumber;
                    CarColorEntry.Text = contract.CarColor;
                    CarRegistrationNumberEntry.Text = contract.CarRegistrationNumber;
                    
                    SellerFullNameEntry.Text = contract.SellerFullName;
                    SellerPassportSeriesEntry.Text = contract.SellerPassportSeries;
                    SellerPassportNumberEntry.Text = contract.SellerPassportNumber;
                    SellerPassportIssuedByEntry.Text = contract.SellerPassportIssuedBy;
                    SellerPassportIssuedDatePicker.Date = contract.SellerPassportIssuedDate;
                    SellerAddressEditor.Text = contract.SellerAddress;
                    
                    BuyerFullNameEntry.Text = contract.BuyerFullName;
                    BuyerPassportSeriesEntry.Text = contract.BuyerPassportSeries;
                    BuyerPassportNumberEntry.Text = contract.BuyerPassportNumber;
                    BuyerPassportIssuedByEntry.Text = contract.BuyerPassportIssuedBy;
                    BuyerPassportIssuedDatePicker.Date = contract.BuyerPassportIssuedDate;
                    BuyerAddressEditor.Text = contract.BuyerAddress;
                    
                    SalePriceEntry.Text = contract.SalePrice.ToString();
                    PaymentMethodPicker.SelectedItem = contract.PaymentMethod;
                    AdditionalTermsEditor.Text = contract.AdditionalTerms;
                    
                    SellerSignedCheckBox.IsChecked = contract.SellerSigned;
                    BuyerSignedCheckBox.IsChecked = contract.BuyerSigned;
                }
            }
        }

        private async void OnSaveContractClicked(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(CarBrandEntry.Text) ||
                string.IsNullOrWhiteSpace(CarModelEntry.Text) ||
                string.IsNullOrWhiteSpace(CarYearEntry.Text) ||
                string.IsNullOrWhiteSpace(CarVinEntry.Text) ||
                string.IsNullOrWhiteSpace(SellerFullNameEntry.Text) ||
                string.IsNullOrWhiteSpace(BuyerFullNameEntry.Text) ||
                string.IsNullOrWhiteSpace(SalePriceEntry.Text))
            {
                await DisplayAlert("Ошибка", "Пожалуйста, заполните все обязательные поля", "OK");
                return;
            }

            if (!int.TryParse(CarYearEntry.Text, out int year))
            {
                await DisplayAlert("Ошибка", "Введите корректный год", "OK");
                return;
            }

            if (!decimal.TryParse(SalePriceEntry.Text, out decimal price) || price <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректную цену", "OK");
                return;
            }

            var contract = new SaleContract
            {
                ContractDate = ContractDatePicker.Date,
                CarBrand = CarBrandEntry.Text.Trim(),
                CarModel = CarModelEntry.Text.Trim(),
                CarYear = year,
                CarVin = CarVinEntry.Text.Trim(),
                CarEngineNumber = CarEngineNumberEntry.Text?.Trim() ?? string.Empty,
                CarBodyNumber = CarBodyNumberEntry.Text?.Trim() ?? string.Empty,
                CarColor = CarColorEntry.Text.Trim(),
                CarRegistrationNumber = CarRegistrationNumberEntry.Text?.Trim() ?? string.Empty,
                
                SellerFullName = SellerFullNameEntry.Text.Trim(),
                SellerPassportSeries = SellerPassportSeriesEntry.Text?.Trim() ?? string.Empty,
                SellerPassportNumber = SellerPassportNumberEntry.Text?.Trim() ?? string.Empty,
                SellerPassportIssuedBy = SellerPassportIssuedByEntry.Text?.Trim() ?? string.Empty,
                SellerPassportIssuedDate = SellerPassportIssuedDatePicker.Date,
                SellerAddress = SellerAddressEditor.Text?.Trim() ?? string.Empty,
                
                BuyerFullName = BuyerFullNameEntry.Text.Trim(),
                BuyerPassportSeries = BuyerPassportSeriesEntry.Text?.Trim() ?? string.Empty,
                BuyerPassportNumber = BuyerPassportNumberEntry.Text?.Trim() ?? string.Empty,
                BuyerPassportIssuedBy = BuyerPassportIssuedByEntry.Text?.Trim() ?? string.Empty,
                BuyerPassportIssuedDate = BuyerPassportIssuedDatePicker.Date,
                BuyerAddress = BuyerAddressEditor.Text?.Trim() ?? string.Empty,
                
                SalePrice = price,
                PaymentMethod = PaymentMethodPicker.SelectedItem?.ToString() ?? string.Empty,
                AdditionalTerms = AdditionalTermsEditor.Text?.Trim() ?? string.Empty,
                
                SellerSigned = SellerSignedCheckBox.IsChecked,
                BuyerSigned = BuyerSignedCheckBox.IsChecked
            };

            if (_contractId.HasValue)
            {
                contract.Id = _contractId.Value;
                _contractService.UpdateContract(contract);
                await DisplayAlert("Успех", "Договор обновлен", "OK");
            }
            else
            {
                _contractService.AddContract(contract);
                await DisplayAlert("Успех", "Договор сохранен", "OK");
            }

            await Navigation.PopAsync();
        }

        private async void OnPrintContractClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Печать", "Функция печати будет реализована в будущей версии", "OK");
        }
    }
}
