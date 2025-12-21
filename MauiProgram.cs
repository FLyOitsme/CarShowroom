using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DataLayer.Entities;
using CarShowroom.Services;
using CarShowroom.Interfaces;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace CarShowroom
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Устанавливаем русскую культуру для приложения
            var culture = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Загрузка конфигурации из appsettings.json
            string connectionString = "Host=localhost;Database=bfd;Username=postgres;Password=1357";
            
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("CarShowroom.appsettings.json");
                
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    var jsonContent = reader.ReadToEnd();
                    var jsonDoc = JsonDocument.Parse(jsonContent);
                    
                    if (jsonDoc.RootElement.TryGetProperty("ConnectionStrings", out var connStrings))
                    {
                        if (connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                        {
                            connectionString = defaultConn.GetString() ?? connectionString;
                        }
                    }
                }
            }
            catch
            {
                // Если файл не найден, используем значения по умолчанию
            }
            
            builder.Services.AddDbContext<CarShowroomDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Регистрация сервисов через интерфейсы
            builder.Services.AddScoped<ICarService, CarService>();
            builder.Services.AddScoped<ISaleService, SaleService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPdfContractService, PdfContractService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddSingleton<IImageSearchService, ImageSearchService>();
            
            // Регистрация FileSaver
            builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);

            // Регистрация ViewModels
            builder.Services.AddTransient<ViewModels.MainPageViewModel>();
            builder.Services.AddTransient<ViewModels.CarDetailsPageViewModel>();
            builder.Services.AddTransient<ViewModels.AddEditCarPageViewModel>();
            builder.Services.AddTransient<ViewModels.CreateSalePageViewModel>();
            builder.Services.AddTransient<ViewModels.SalesListPageViewModel>();
            builder.Services.AddTransient<ViewModels.AddEditDiscountPageViewModel>(sp =>
            {
                var saleService = sp.GetRequiredService<ISaleService>();
                var carService = sp.GetRequiredService<ICarService>();
                return new ViewModels.AddEditDiscountPageViewModel(saleService, carService);
            });
            builder.Services.AddTransient<ViewModels.DiscountsListPageViewModel>();
            builder.Services.AddTransient<ViewModels.ReportsPageViewModel>();
            builder.Services.AddTransient<ViewModels.ContractPreviewPageViewModel>();
            
            // Регистрация FileSaver для ViewModels
            builder.Services.AddSingleton<CommunityToolkit.Maui.Storage.IFileSaver>(
                CommunityToolkit.Maui.Storage.FileSaver.Default);

            // Регистрация страниц
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CarDetailsPage>();
            builder.Services.AddTransient<AddEditCarPage>();
            builder.Services.AddTransient<CreateSalePage>();
            builder.Services.AddTransient<SalesListPage>();
            builder.Services.AddTransient<AddEditDiscountPage>();
            builder.Services.AddTransient<DiscountsListPage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<ContractPreviewPage>();

            var app = builder.Build();
            
            // Создаем базу данных при старте приложения, если она не существует
            InitializeDatabase(app.Services);
            
            return app;
        }

        private static async void InitializeDatabase(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CarShowroomDbContext>();
                
                // EnsureCreatedAsync создаст базу данных и все таблицы, если они не существуют
                // Это единственная функция EF Core, необходимая для создания БД и схемы
                var created = await dbContext.Database.EnsureCreatedAsync();
                
                if (created)
                {
                    System.Diagnostics.Debug.WriteLine("База данных и таблицы успешно созданы.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("База данных уже существует.");
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем запуск приложения
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании базы данных: {ex.Message}");
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Тип ошибки: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
#endif
            }
        }
    }
}
