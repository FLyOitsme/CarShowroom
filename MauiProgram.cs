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

namespace CarShowroom
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
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
            string connectionString = "Host=localhost;Database=CarShowroomBD;Username=postgres;Password=1357";
            
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
                
                // Проверяем, может ли приложение подключиться к базе данных
                var canConnect = await dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    System.Diagnostics.Debug.WriteLine("Не удалось подключиться к базе данных. Убедитесь, что PostgreSQL запущен и база данных существует.");
                    return;
                }
                
                // Создаем базу данных и все таблицы, если они не существуют
                // EnsureCreated создает БД и все таблицы на основе моделей из OnModelCreating
                var created = await dbContext.Database.EnsureCreatedAsync();
                
                if (created)
                {
                    System.Diagnostics.Debug.WriteLine("База данных успешно создана.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("База данных уже существует.");
                    // Обновляем структуру существующих таблиц, если нужно
                    await UpdateDatabaseSchemaAsync(dbContext);
                }
            }
            catch (Npgsql.PostgresException pgEx)
            {
                // Специфичная обработка ошибок PostgreSQL
                System.Diagnostics.Debug.WriteLine($"Ошибка PostgreSQL: {pgEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Код ошибки: {pgEx.SqlState}");
                
                if (pgEx.SqlState == "3D000") // База данных не существует
                {
                    System.Diagnostics.Debug.WriteLine("База данных не существует. Создайте её вручную или проверьте строку подключения.");
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

        private static async Task UpdateDatabaseSchemaAsync(CarShowroomDbContext dbContext)
        {
            try
            {
                // Проверяем и добавляем столбцы StartDate и EndDate в таблицу Discount, если их нет
                var connection = dbContext.Database.GetDbConnection();
                await connection.OpenAsync();
                
                try
                {
                    using var command = connection.CreateCommand();
                    
                    // Проверяем существование столбца StartDate
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM information_schema.columns 
                        WHERE table_name = 'Discount' 
                        AND column_name = 'StartDate'";
                    
                    var startDateExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                    
                    // Добавляем StartDate, если его нет
                    if (!startDateExists)
                    {
                        command.CommandText = @"ALTER TABLE ""Discount"" ADD COLUMN ""StartDate"" DATE";
                        await command.ExecuteNonQueryAsync();
                        System.Diagnostics.Debug.WriteLine("Столбец StartDate добавлен в таблицу Discount.");
                    }
                    
                    // Проверяем существование столбца EndDate
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM information_schema.columns 
                        WHERE table_name = 'Discount' 
                        AND column_name = 'EndDate'";
                    
                    var endDateExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                    
                    // Добавляем EndDate, если его нет
                    if (!endDateExists)
                    {
                        command.CommandText = @"ALTER TABLE ""Discount"" ADD COLUMN ""EndDate"" DATE";
                        await command.ExecuteNonQueryAsync();
                        System.Diagnostics.Debug.WriteLine("Столбец EndDate добавлен в таблицу Discount.");
                    }
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении схемы базы данных: {ex.Message}");
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
#endif
            }
        }
    }
}
