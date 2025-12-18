using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using DataLayer.Entities;
using CommunityToolkit.Maui.Storage;

namespace CarShowroom.ViewModels
{
    public partial class SalesListPageViewModel : ObservableObject
    {
        private readonly ISaleService _saleService;
        private readonly IPdfContractService _pdfContractService;
        private readonly IUserService _userService;
        private readonly IFileSaver? _fileSaver;

        [ObservableProperty]
        private List<Sale> _sales = new();

        [ObservableProperty]
        private Sale? _selectedSale;

        public SalesListPageViewModel(ISaleService saleService, IPdfContractService pdfContractService, IUserService userService, IFileSaver? fileSaver = null)
        {
            _saleService = saleService;
            _pdfContractService = pdfContractService;
            _userService = userService;
            _fileSaver = fileSaver;
        }

        [RelayCommand]
        private async Task LoadSalesAsync()
        {
            try
            {
                Sales = await _saleService.GetAllSalesAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить продажи: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SaleSelectedAsync(Sale? sale)
        {
            if (sale != null)
            {
                var action = await Shell.Current.DisplayActionSheet(
                    "Выберите действие",
                    "Отмена",
                    null,
                    "Показать детали",
                    "Сгенерировать договор");
                
                if (action == "Сгенерировать договор")
                {
                    await GenerateContractAsync(sale);
                }
                else if (action == "Показать детали")
                {
                    await Shell.Current.DisplayAlert("Продажа",
                        $"Автомобиль: {sale.Car?.Model?.Brand?.Name} {sale.Car?.Model?.Name}\n" +
                        $"Менеджер: {sale.Manager?.Name} {sale.Manager?.Surname}\n" +
                        $"Дата: {sale.Date}\n" +
                        $"Цена: {sale.Cost:N0} ₽",
                        "OK");
                }
                
                SelectedSale = null;
            }
        }

        [RelayCommand]
        private async Task GenerateContractForSaleAsync(Sale sale)
        {
            await GenerateContractAsync(sale);
        }

        private async Task GenerateContractAsync(Sale sale)
        {
            try
            {
                // Получаем полную информацию о продаже
                var fullSale = await _saleService.GetSaleByIdAsync(sale.Id);
                if (fullSale == null || fullSale.Car == null || fullSale.Manager == null)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить данные о продаже", "OK");
                    return;
                }

                // Запрашиваем данные клиента
                var clientName = await Shell.Current.DisplayPromptAsync(
                    "Данные клиента",
                    "Введите ФИО клиента:",
                    "OK",
                    "Отмена",
                    "ФИО клиента",
                    -1,
                    Keyboard.Default);

                if (string.IsNullOrWhiteSpace(clientName))
                {
                    await Shell.Current.DisplayAlert("Информация", "Генерация договора отменена", "OK");
                    return;
                }

                // Парсим ФИО и создаем объект клиента
                var nameParts = clientName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var client = new User
                {
                    Surname = nameParts.Length > 0 ? nameParts[0] : null,
                    Name = nameParts.Length > 1 ? nameParts[1] : null,
                    Patronyc = nameParts.Length > 2 ? nameParts[2] : null
                };

                // Получаем опции и скидки
                var additions = await _saleService.GetSaleAdditionsAsync(sale.Id);
                var discounts = await _saleService.GetSaleDiscountsAsync(sale.Id);

                // Рассчитываем цены
                // В базе хранится итоговая цена (после всех скидок и с опциями)
                decimal finalPrice = (decimal)(fullSale.Cost ?? 0);
                
                // Рассчитываем стоимость опций
                decimal additionsCost = additions.Sum(a => (decimal)(a.Cost ?? 0));
                
                // Вычитаем стоимость опций из итоговой цены, чтобы получить цену с опциями до скидок
                decimal priceWithAdditions = finalPrice - additionsCost;
                
                // Применяем скидки обратно для расчета базовой цены автомобиля
                var discountIds = discounts.Select(d => d.Id).ToList();
                decimal basePrice = priceWithAdditions;
                if (discountIds.Any() && priceWithAdditions > 0)
                {
                    basePrice = await _saleService.CalculateOriginalPriceAsync(priceWithAdditions, discountIds);
                }
                
                // Если расчет не удался, используем итоговую цену как базовую
                if (basePrice <= 0)
                {
                    basePrice = finalPrice - additionsCost;
                }

                // Генерируем PDF
                var pdfBytes = _pdfContractService.GenerateContract(
                    fullSale,
                    client,
                    fullSale.Manager,
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
                    else if (fileSaverResult.Exception != null)
                    {
                        if (fileSaverResult.Exception.Message.Contains("cancel", StringComparison.OrdinalIgnoreCase) ||
                            fileSaverResult.Exception.Message.Contains("отмен", StringComparison.OrdinalIgnoreCase))
                        {
                            await Shell.Current.DisplayAlert("Информация", 
                                "Сохранение файла отменено пользователем", 
                                "OK");
                            return;
                        }
                    }
                }
                
                // Если FileSaver не доступен или произошла ошибка, сохраняем в кэш
                var cacheDir = FileSystem.CacheDirectory;
                var filePath = Path.Combine(cacheDir, fileName);
                await File.WriteAllBytesAsync(filePath, pdfBytes);
                
                await Shell.Current.DisplayAlert("Информация", 
                    $"PDF договор сохранен в кэш:\n{filePath}\n\n" +
                    $"Размер файла: {pdfBytes.Length / 1024} КБ", 
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
        private async Task AddSaleAsync()
        {
            await Shell.Current.GoToAsync("CreateSalePage");
        }
    }
}

