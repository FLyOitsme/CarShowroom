using CarShowroom.ViewModels;
using System.Linq;

namespace CarShowroom
{
    public partial class SalesListPage : ContentPage
    {
        public SalesListPageViewModel ViewModel { get; }

        public SalesListPage(SalesListPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Проверяем текущее состояние навигации Shell
            // Если открыта страница создания продажи, закрываем её
            // Это гарантирует, что при возврате на раздел продаж всегда открывается список продаж
            try
            {
                var currentLocation = Shell.Current.CurrentState?.Location?.ToString() ?? string.Empty;
                
                // Если текущий маршрут содержит CreateSalePage, закрываем её
                if (currentLocation.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                {
                    // Небольшая задержка, чтобы убедиться, что навигация завершена
                    await Task.Delay(150);
                    
                    // Пытаемся закрыть страницу создания продажи через навигацию назад
                    // Если это не сработает, используем абсолютный путь
                    try
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                    catch
                    {
                        // Если навигация назад не сработала, используем абсолютный путь
                        await Shell.Current.GoToAsync("///SalesListPage");
                    }
                }
                
                // Дополнительная проверка: ищем CreateSalePage в стеке навигации
                var navigationStack = Shell.Current.Navigation.NavigationStack;
                if (navigationStack != null && navigationStack.Count > 0)
                {
                    var createSalePage = navigationStack.OfType<CreateSalePage>().FirstOrDefault();
                    if (createSalePage != null)
                    {
                        // Закрываем страницу создания продажи
                        await Task.Delay(100);
                        await Shell.Current.GoToAsync("///SalesListPage");
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки навигации
            }
            
            // Обновляем список продаж при появлении страницы
            await ViewModel.LoadSalesCommand.ExecuteAsync(null);
        }

        private async void OnSaleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is DataLayer.Entities.Sale selectedSale)
            {
                await ViewModel.SaleSelectedCommand.ExecuteAsync(selectedSale);
            }
        }
    }
}

