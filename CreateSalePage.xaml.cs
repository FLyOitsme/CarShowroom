using CarShowroom.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.Linq;

namespace CarShowroom
{
    [QueryProperty(nameof(CarId), "carId")]
    public partial class CreateSalePage : ContentPage
    {
        public CreateSalePageViewModel ViewModel { get; }

        public string CarId { get; set; } = string.Empty;
        
        // Статический флаг для отслеживания активной страницы создания продажи
        public static CreateSalePage? CurrentInstance { get; private set; }

        public CreateSalePage(CreateSalePageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
            
            // Подписываемся на событие автоматического применения скидок
            ViewModel.DiscountsAutoApplied += OnDiscountsAutoApplied;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Устанавливаем текущий экземпляр страницы
            CurrentInstance = this;
            
            // Подписываемся на событие навигации Shell для отслеживания переходов между разделами
            Shell.Current.Navigated += OnShellNavigated;
            
            // Подписываемся на событие Navigating для блокировки навигации
            Shell.Current.Navigating += OnShellNavigating;
            
            long? carId = null;
            if (!string.IsNullOrEmpty(CarId) && long.TryParse(CarId, out long id))
            {
                carId = id;
            }
            ViewModel.Initialize(carId);
        }
        
        private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            // Проверяем, заблокирована ли навигация
            if (ViewModel.IsNavigationLocked && !ViewModel.IsCancelling)
            {
                // Получаем маршрут назначения
                var target = e.Target?.Location?.ToString() ?? string.Empty;
                
                // Разрешаем навигацию только внутри страницы создания продажи (например, обновление параметров)
                if (target.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                {
                    return; // Разрешаем навигацию внутри CreateSalePage
                }
                
                // Отменяем навигацию
                e.Cancel();
                
                // Показываем предупреждение
                var result = await Shell.Current.DisplayAlert(
                    "Внимание",
                    "Вы не можете покинуть страницу создания продажи. Пожалуйста, создайте продажу или отмените операцию.",
                    "OK",
                    "Отменить");
                
                // Если пользователь выбрал "Отменить", вызываем команду отмены
                if (!result)
                {
                    await ViewModel.CancelCommand.ExecuteAsync(null);
                }
            }
        }
        
        protected override bool OnBackButtonPressed()
        {
            // Блокируем кнопку назад, если навигация заблокирована
            if (ViewModel.IsNavigationLocked && !ViewModel.IsCancelling)
            {
                // Показываем предупреждение
                _ = Task.Run(async () =>
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        var result = await Shell.Current.DisplayAlert(
                            "Внимание",
                            "Вы не можете покинуть страницу создания продажи. Пожалуйста, создайте продажу или отмените операцию.",
                            "OK",
                            "Отменить");
                        
                        if (!result)
                        {
                            await ViewModel.CancelCommand.ExecuteAsync(null);
                        }
                    });
                });
                
                return true; // Блокируем навигацию назад
            }
            
            return base.OnBackButtonPressed();
        }
        
        private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            try
            {
                // Проверяем, перешли ли мы в другой раздел (не SalesListPage)
                var currentLocation = Shell.Current.CurrentState?.Location?.ToString() ?? string.Empty;
                
                // Если мы не находимся в разделе продаж (SalesListPage или его дочерние страницы), закрываем страницу создания продажи
                if (!string.IsNullOrEmpty(currentLocation) && 
                    !currentLocation.Contains("SalesListPage", StringComparison.OrdinalIgnoreCase) &&
                    !currentLocation.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                {
                    // Отписываемся от события перед закрытием
                    Shell.Current.Navigated -= OnShellNavigated;
                    
                    // Небольшая задержка для завершения текущей навигации
                    await Task.Delay(50);
                    
                    // Закрываем страницу, возвращаясь на родительскую
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch
            {
                // Игнорируем ошибки навигации
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Сбрасываем текущий экземпляр страницы
            if (CurrentInstance == this)
            {
                CurrentInstance = null;
            }
            
            // Отписываемся от события навигации Shell
            Shell.Current.Navigated -= OnShellNavigated;
            Shell.Current.Navigating -= OnShellNavigating;
            
            // Отписываемся от события, чтобы избежать утечек памяти
            if (ViewModel != null)
            {
                ViewModel.DiscountsAutoApplied -= OnDiscountsAutoApplied;
            }
            
            try
            {
                // При выходе со страницы всегда вызываем метод очистки данных
                // Это гарантирует, что все операции с БД будут отменены
                if (ViewModel != null)
                {
                    // Вызываем метод очистки данных напрямую, без навигации
                    ViewModel.ClearData();
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не крашим приложение
                System.Diagnostics.Debug.WriteLine($"Ошибка при очистке данных в OnDisappearing: {ex.Message}");
            }
            finally
            {
                // Сбрасываем флаг после обработки
                if (ViewModel != null)
                {
                    ViewModel.IsCancelling = false;
                }
            }
        }

        private void OnDiscountsAutoApplied()
        {
            // Проверяем, что ViewModel еще существует (страница не закрыта)
            if (ViewModel == null) return;
            
            // Выбор скидки теперь обрабатывается через чекбоксы в DiscountSelectionItems
            // Не нужно программно устанавливать выбор, так как это делается в ViewModel
        }

        private void OnAdditionsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is CollectionView collectionView)
            {
                ViewModel.OnAdditionSelectionChanged(collectionView.SelectedItems);
            }
        }

        private void OnDiscountsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is CollectionView collectionView)
            {
                // Если выбрана скидка, устанавливаем её в ViewModel
                if (e.CurrentSelection?.FirstOrDefault() is DataLayer.Entities.Discount selectedDiscount)
                {
                    ViewModel.SelectedDiscount = selectedDiscount;
                }
                else if (e.CurrentSelection?.Count == 0)
                {
                    // Если выбор снят, очищаем выбранную скидку
                    ViewModel.SelectedDiscount = null;
                }
                
                ViewModel.OnDiscountSelectionChanged(sender, e);
            }
        }

        private void OnDiscountCheckboxChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is ViewModels.DiscountSelectionItem item)
            {
                ViewModel.OnDiscountCheckboxChanged(item);
            }
        }

        private void OnClientSelected(object sender, SelectionChangedEventArgs e)
        {
            // Выбор клиента обрабатывается через привязку SelectedItem и OnSelectedClientChanged в ViewModel
            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null; // Сбрасываем выбор для возможности повторного выбора
            }
        }

        private void OnClientSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SearchClientsCommand.ExecuteAsync(null);
        }

        private void OnSearchButtonPressed(object sender, EventArgs e)
        {
            ViewModel.SearchClientsCommand.ExecuteAsync(null);
        }
    }
}
