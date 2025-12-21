using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using DataLayer.Entities;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

namespace CarShowroom.ViewModels
{
    public partial class ContractPreviewPageViewModel : ObservableObject
    {
        private readonly IPdfContractService _pdfContractService;
        private readonly ISaleService _saleService;
        private readonly IFileSaver? _fileSaver;

        [ObservableProperty]
        private Sale? _sale;

        [ObservableProperty]
        private Client? _client;

        [ObservableProperty]
        private User? _manager;

        [ObservableProperty]
        private Car? _car;

        [ObservableProperty]
        private List<Addition> _additions = new();

        [ObservableProperty]
        private List<Discount> _discounts = new();

        [ObservableProperty]
        private decimal _basePrice;

        [ObservableProperty]
        private decimal _finalPrice;

        [ObservableProperty]
        private string _contractNumber = string.Empty;

        [ObservableProperty]
        private string _contractDate = string.Empty;

        [ObservableProperty]
        private string _carInfo = string.Empty;

        [ObservableProperty]
        private string _priceInfo = string.Empty;

        [ObservableProperty]
        private string _additionsInfo = string.Empty;

        [ObservableProperty]
        private string _discountsInfo = string.Empty;

        [ObservableProperty]
        private decimal _discountAmount;

        private long _saleId;

        public ContractPreviewPageViewModel(IPdfContractService pdfContractService, ISaleService saleService, IFileSaver? fileSaver = null)
        {
            _pdfContractService = pdfContractService;
            _saleService = saleService;
            _fileSaver = fileSaver;
        }

        public void Initialize(long saleId, decimal basePrice, decimal finalPrice)
        {
            _saleId = saleId;
            BasePrice = basePrice;
            FinalPrice = finalPrice;
            LoadContractDataCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task LoadContractDataAsync()
        {
            try
            {
                Sale = await _saleService.GetSaleByIdAsync((int)_saleId);
                if (Sale == null || Sale.Car == null || Sale.Client == null || Sale.Manager == null)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить данные о договоре", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Car = Sale.Car;
                Client = Sale.Client;
                Manager = Sale.Manager;
                Additions = await _saleService.GetSaleAdditionsAsync((int)_saleId);
                Discounts = await _saleService.GetSaleDiscountsAsync((int)_saleId);

                ContractNumber = $"№ {Sale.Id}";
                ContractDate = Sale.Date?.ToString("dd.MM.yyyy") ?? string.Empty;

                // Формируем информацию об автомобиле
                CarInfo = $"Марка: {Car.Model?.Brand?.Name ?? "Неизвестно"}\n" +
                         $"Модель: {Car.Model?.Name ?? "Неизвестно"}\n" +
                         $"Год выпуска: {Car.Year}\n" +
                         $"Цвет: {Car.Color ?? "Не указан"}\n" +
                         $"Объем двигателя: {(Car.EngVol.HasValue ? $"{Car.EngVol:F1} л" : "Не указан")}\n" +
                         $"Пробег: {(Car.Mileage.HasValue ? $"{Car.Mileage:N0} км" : "Не указан")}\n" +
                         $"Коробка передач: {Car.Transmission?.Name ?? "Не указана"}\n" +
                         $"Тип кузова: {Car.Type?.Name ?? "Не указан"}\n" +
                         $"Состояние: {Car.Condition?.Name ?? "Не указано"}";

                // Формируем информацию о цене
                var additionsCost = Additions.Sum(a => (decimal)(a.Cost ?? 0));
                var priceWithAdditions = BasePrice + additionsCost;
                DiscountAmount = priceWithAdditions - FinalPrice;
                
                PriceInfo = $"Базовая цена автомобиля: {BasePrice:N0} ₽";
                if (Additions.Any())
                {
                    PriceInfo += $"\nСтоимость дополнительных опций: {additionsCost:N0} ₽";
                }
                if (Discounts.Any() && DiscountAmount > 0)
                {
                    PriceInfo += $"\nСумма скидки: {DiscountAmount:N0} ₽";
                }
                PriceInfo += $"\nИтоговая цена: {FinalPrice:N0} ₽";

                // Формируем информацию о дополнительных опциях
                if (Additions.Any())
                {
                    AdditionsInfo = string.Join("\n", Additions.Select(a => $"• {a.Name}: {a.Cost:N0} ₽"));
                }
                else
                {
                    AdditionsInfo = "Нет";
                }

                // Формируем информацию о скидках
                if (Discounts.Any())
                {
                    var discountsList = Discounts.Select(d => 
                    {
                        var discountAmount = DiscountAmount > 0 && Discounts.Count == 1 
                            ? $" ({DiscountAmount:N0} ₽)" 
                            : "";
                        return $"• {d.Name}: {d.Cost}%{discountAmount}";
                    });
                    DiscountsInfo = string.Join("\n", discountsList);
                }
                else
                {
                    DiscountsInfo = "Нет";
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task GeneratePdfAsync()
        {
            if (Sale == null || Car == null || Client == null || Manager == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не все данные загружены", "OK");
                return;
            }

            try
            {
                // Генерируем PDF
                var pdfBytes = _pdfContractService.GenerateContract(
                    Sale,
                    Client,
                    Manager,
                    Car,
                    Additions,
                    Discounts,
                    BasePrice,
                    FinalPrice);

                // Сохраняем PDF
                await SavePdfFileAsync(pdfBytes, $"Договор_№{Sale.Id}_{DateTime.Now:yyyyMMdd}.pdf");
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
                        if (fileSaverResult.Exception != null)
                        {
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
                    await SaveToCacheAsync(pdfBytes, fileName, "FileSaver не доступен");
                }
            }
            catch (Exception ex)
            {
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

        [RelayCommand]
        private async Task CloseAsync()
        {
            // Возвращаемся на страницу списка продаж
            await Shell.Current.GoToAsync("///SalesListPage");
        }
    }
}

