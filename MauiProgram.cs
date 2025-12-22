using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Dom;
using CarShowroom.Services;
using CarShowroom.Interfaces;
using CarShowroom.Repositories;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using MSDI = Microsoft.Extensions.DependencyInjection;

namespace CarShowroom
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
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
            }
            
            builder.Services.AddDbContext<CarShowroomDbContext>(options =>
                options.UseNpgsql(connectionString));

            MSDI.ServiceCollectionServiceExtensions.AddScoped<IRepository<CarType>, Repository<CarType>>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IRepository<ConditionType>, Repository<ConditionType>>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IRepository<EngineType>, Repository<EngineType>>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IRepository<Transmission>, Repository<Transmission>>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IRepository<Wdtype>, Repository<Wdtype>>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IRepository<RoleType>, Repository<RoleType>>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<ICarRepository, CarRepository>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IBrandRepository, BrandRepository>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IModelRepository, ModelRepository>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<ISaleRepository, SaleRepository>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IUserRepository, UserRepository>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IClientRepository, ClientRepository>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IAdditionRepository, AdditionRepository>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IDiscountRepository, DiscountRepository>(builder.Services);

            MSDI.ServiceCollectionServiceExtensions.AddScoped<ICarService, CarService>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<ISaleService, SaleService>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IUserService, UserService>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IPdfContractService, PdfContractService>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddScoped<IReportService, ReportService>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddSingleton<IImageSearchService, ImageSearchService>(builder.Services);
            
            MSDI.ServiceCollectionServiceExtensions.AddSingleton<IFileSaver>(builder.Services, FileSaver.Default);

            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.MainPageViewModel>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.CarDetailsPageViewModel>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.AddEditCarPageViewModel>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.CreateSalePageViewModel>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.SalesListPageViewModel>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.AddEditDiscountPageViewModel>(builder.Services, sp =>
            {
                var saleService = sp.GetRequiredService<ISaleService>();
                var carService = sp.GetRequiredService<ICarService>();
                return new ViewModels.AddEditDiscountPageViewModel(saleService, carService);
            });
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.DiscountsListPageViewModel>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.ReportsPageViewModel>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ViewModels.ContractPreviewPageViewModel>(builder.Services);
            
            MSDI.ServiceCollectionServiceExtensions.AddSingleton<CommunityToolkit.Maui.Storage.IFileSaver>(
                builder.Services, CommunityToolkit.Maui.Storage.FileSaver.Default);

            MSDI.ServiceCollectionServiceExtensions.AddTransient<MainPage>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<CarDetailsPage>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<AddEditCarPage>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<CreateSalePage>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<SalesListPage>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<AddEditDiscountPage>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<DiscountsListPage>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ReportsPage>(builder.Services);
            MSDI.ServiceCollectionServiceExtensions.AddTransient<ContractPreviewPage>(builder.Services);

            var app = builder.Build();
            
            return app;
        }
    }
}
