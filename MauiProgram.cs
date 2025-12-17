using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DataLayer.Entities;
using CarShowroom.Services;

namespace CarShowroom
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Настройка подключения к базе данных
            var connectionString = "Host=localhost;Database=CarShowroomBD;Username=postgres;Password=123";
            builder.Services.AddDbContext<CarShowroomDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Регистрация сервисов
            builder.Services.AddScoped<CarService>();
            builder.Services.AddScoped<SaleService>();
            builder.Services.AddScoped<UserService>();

            // Регистрация ViewModels
            builder.Services.AddTransient<ViewModels.MainPageViewModel>();
            builder.Services.AddTransient<ViewModels.CarDetailsPageViewModel>();
            builder.Services.AddTransient<ViewModels.AddEditCarPageViewModel>();
            builder.Services.AddTransient<ViewModels.CreateSalePageViewModel>();
            builder.Services.AddTransient<ViewModels.SalesListPageViewModel>();

            // Регистрация страниц
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CarDetailsPage>();
            builder.Services.AddTransient<AddEditCarPage>();
            builder.Services.AddTransient<CreateSalePage>();
            builder.Services.AddTransient<SalesListPage>();

            return builder.Build();
        }
    }
}
