using CarShowroom.Models;
using CarShowroom.Services;

namespace CarShowroom
{
    public partial class SaleContractPage : ContentPage
    {
        private readonly SaleContractService _contractService;
        private readonly CarService _carService;
        private int? _carId;
        private int? _contractId;

        public SaleContractPage(int? carId = null, int? contractId = null)
        {
            InitializeComponent();
            _contractService = new SaleContractService();
            _carService = new CarService();
            _carId = carId;
            _contractId = contractId;

            ContractDatePicker.Date = DateTime.Now;

            // Предзаполняем данные продавца (автосалона)
            LoadSellerData();

            if (_contractId.HasValue)
            {
                Title = "Редактировать договор";
                LoadContractData();
            }
            else if (_carId.HasValue)
            {
                LoadCarData();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Убеждаемся, что данные загружены при появлении страницы
            if (!_contractId.HasValue)
            {
                LoadSellerData();
                if (_carId.HasValue)
                {
                    LoadCarData();
                }
            }
        }

        private void LoadSellerData()
        {
            // Предзаполняем данные продавца только если это новый договор
            if (!_contractId.HasValue)
            {
                SellerCompanyNameEntry.Text = ShowroomService.CompanyName;
                SellerInnEntry.Text = ShowroomService.Inn;
                SellerOgrnEntry.Text = ShowroomService.Ogrn;
                SellerAddressEditor.Text = ShowroomService.Address;
                SellerDirectorNameEntry.Text = ShowroomService.DirectorName;
                SellerDirectorPositionEntry.Text = ShowroomService.DirectorPosition;
            }
        }

        private void LoadCarData()
        {
            if (_carId.HasValue)
            {
                var car = _carService.GetCarById(_carId.Value);
                if (car != null)
                {
                    // Заполняем все поля данными автомобиля
                    CarBrandEntry.Text = car.Brand ?? string.Empty;
                    CarModelEntry.Text = car.Model ?? string.Empty;
                    CarYearEntry.Text = car.Year > 0 ? car.Year.ToString() : string.Empty;
                    CarColorEntry.Text = car.Color ?? string.Empty;
                    CarVinEntry.Text = car.Vin ?? string.Empty;
                    CarEngineNumberEntry.Text = car.EngineNumber ?? string.Empty;
                    CarBodyNumberEntry.Text = car.BodyNumber ?? string.Empty;
                    // Гос. номер не заполняем, так как машины новые
                    CarRegistrationNumberEntry.Text = string.Empty;
                    SalePriceEntry.Text = car.Price > 0 ? car.Price.ToString("F0") : string.Empty;
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
                    
                    SellerCompanyNameEntry.Text = contract.SellerCompanyName;
                    SellerInnEntry.Text = contract.SellerInn;
                    SellerOgrnEntry.Text = contract.SellerOgrn;
                    SellerAddressEditor.Text = contract.SellerAddress;
                    SellerDirectorNameEntry.Text = contract.SellerDirectorName;
                    SellerDirectorPositionEntry.Text = contract.SellerDirectorPosition;
                    
                    BuyerFullNameEntry.Text = contract.BuyerFullName;
                    BuyerPassportSeriesEntry.Text = contract.BuyerPassportSeries;
                    BuyerPassportNumberEntry.Text = contract.BuyerPassportNumber;
                    BuyerPassportIssuedByEntry.Text = contract.BuyerPassportIssuedBy;
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
                string.IsNullOrWhiteSpace(SellerCompanyNameEntry.Text) ||
                string.IsNullOrWhiteSpace(SellerInnEntry.Text) ||
                string.IsNullOrWhiteSpace(SellerOgrnEntry.Text) ||
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
                
                SellerCompanyName = SellerCompanyNameEntry.Text.Trim(),
                SellerInn = SellerInnEntry.Text.Trim(),
                SellerOgrn = SellerOgrnEntry.Text.Trim(),
                SellerAddress = SellerAddressEditor.Text?.Trim() ?? string.Empty,
                SellerDirectorName = SellerDirectorNameEntry.Text?.Trim() ?? string.Empty,
                SellerDirectorPosition = SellerDirectorPositionEntry.Text?.Trim() ?? string.Empty,
                
                BuyerFullName = BuyerFullNameEntry.Text.Trim(),
                BuyerPassportSeries = BuyerPassportSeriesEntry.Text?.Trim() ?? string.Empty,
                BuyerPassportNumber = BuyerPassportNumberEntry.Text?.Trim() ?? string.Empty,
                BuyerPassportIssuedBy = BuyerPassportIssuedByEntry.Text?.Trim() ?? string.Empty,
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
            // Валидация перед показом превью
            if (string.IsNullOrWhiteSpace(CarBrandEntry.Text) ||
                string.IsNullOrWhiteSpace(CarModelEntry.Text) ||
                string.IsNullOrWhiteSpace(CarYearEntry.Text) ||
                string.IsNullOrWhiteSpace(CarVinEntry.Text) ||
                string.IsNullOrWhiteSpace(SellerCompanyNameEntry.Text) ||
                string.IsNullOrWhiteSpace(BuyerFullNameEntry.Text) ||
                string.IsNullOrWhiteSpace(SalePriceEntry.Text))
            {
                await DisplayAlert("Ошибка", "Пожалуйста, заполните все обязательные поля", "OK");
                return;
            }

            // Показываем превью договора
            var summary = $"ДОГОВОР КУПЛИ-ПРОДАЖИ АВТОМОБИЛЯ\n\n" +
                         $"Дата: {ContractDatePicker.Date:dd.MM.yyyy}\n" +
                         $"г. Москва\n\n" +
                         $"═══════════════════════════════\n\n" +
                         $"ПРОДАВЕЦ:\n" +
                         $"{SellerCompanyNameEntry.Text}\n" +
                         $"ИНН: {SellerInnEntry.Text}\n" +
                         $"ОГРН: {SellerOgrnEntry.Text}\n" +
                         $"Адрес: {SellerAddressEditor.Text}\n\n" +
                         $"═══════════════════════════════\n\n" +
                         $"ПОКУПАТЕЛЬ:\n" +
                         $"{BuyerFullNameEntry.Text}\n" +
                         $"Паспорт: {BuyerPassportSeriesEntry.Text} {BuyerPassportNumberEntry.Text}\n" +
                         $"Выдан: {BuyerPassportIssuedByEntry.Text}\n" +
                         $"Адрес: {BuyerAddressEditor.Text}\n\n" +
                         $"═══════════════════════════════\n\n" +
                         $"АВТОМОБИЛЬ:\n" +
                         $"{CarBrandEntry.Text} {CarModelEntry.Text} ({CarYearEntry.Text})\n" +
                         $"VIN: {CarVinEntry.Text}\n" +
                         $"Цвет: {CarColorEntry.Text}\n\n" +
                         $"═══════════════════════════════\n\n" +
                         $"ЦЕНА: {SalePriceEntry.Text} рублей\n" +
                         $"Способ оплаты: {PaymentMethodPicker.SelectedItem}\n\n" +
                         $"Договор оформлен в приложении.\n" +
                         $"Это демонстрационная версия.";

            await DisplayAlert("Превью договора", summary, "OK");
        }
    }
}
