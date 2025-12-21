namespace CarShowroom
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Регистрация маршрутов для навигации
            Routing.RegisterRoute("MainPage/CarDetailsPage", typeof(CarDetailsPage));
            Routing.RegisterRoute("MainPage/AddEditCarPage", typeof(AddEditCarPage));
            Routing.RegisterRoute("SalesListPage/CreateSalePage", typeof(CreateSalePage));
            Routing.RegisterRoute("AddEditDiscountPage", typeof(AddEditDiscountPage));
            Routing.RegisterRoute("ContractPreviewPage", typeof(ContractPreviewPage));
            
            // Отслеживаем навигацию для закрытия CreateSalePage при переходе в другой раздел
            Navigated += OnShellNavigated;
            
            // Блокируем навигацию, если открыта CreateSalePage и навигация запрещена
            Navigating += OnShellNavigating;
            
            // Подписываемся на сообщения о блокировке навигации
            MessagingCenter.Subscribe<ViewModels.CreateSalePageViewModel, bool>(this, "NavigationLocked", OnNavigationLocked);
        }
        
        private bool _isNavigationLocked = false;
        
        private void OnNavigationLocked(ViewModels.CreateSalePageViewModel sender, bool isLocked)
        {
            _isNavigationLocked = isLocked;
        }
        
        private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            try
            {
                // Проверяем, заблокирована ли навигация
                if (_isNavigationLocked)
                {
                    // Получаем маршрут назначения
                    var target = e.Target?.Location?.ToString() ?? string.Empty;
                    
                    // Разрешаем навигацию только внутри страницы создания продажи
                    if (target.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                    {
                        return; // Разрешаем навигацию внутри CreateSalePage
                    }
                    
                    // Отменяем навигацию
                    e.Cancel();
                    
                    // Проверяем, есть ли активная страница создания продажи
                    if (CreateSalePage.CurrentInstance != null)
                    {
                        var createSalePage = CreateSalePage.CurrentInstance;
                        
                        // Показываем предупреждение
                        var result = await Shell.Current.DisplayAlert(
                            "Внимание",
                            "Вы не можете покинуть страницу создания продажи. Пожалуйста, создайте продажу или отмените операцию.",
                            "OK",
                            "Отменить");
                        
                        // Если пользователь выбрал "Отменить", вызываем команду отмены
                        if (!result)
                        {
                            await createSalePage.ViewModel.CancelCommand.ExecuteAsync(null);
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
        }
        
        private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            try
            {
                // Проверяем, есть ли активная страница создания продажи
                if (CreateSalePage.CurrentInstance != null)
                {
                    var createSalePage = CreateSalePage.CurrentInstance;
                    
                    // Проверяем, заблокирована ли навигация
                    if (createSalePage.ViewModel.IsNavigationLocked && !createSalePage.ViewModel.IsCancelling)
                    {
                        // Получаем текущий маршрут
                        var currentLocation = CurrentState?.Location?.ToString() ?? string.Empty;
                        
                        // Если мы не в разделе продаж, возвращаемся обратно
                        if (!string.IsNullOrEmpty(currentLocation) && 
                            !currentLocation.Contains("SalesListPage", StringComparison.OrdinalIgnoreCase) &&
                            !currentLocation.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                        {
                            // Показываем предупреждение
                            var result = await Shell.Current.DisplayAlert(
                                "Внимание",
                                "Вы не можете покинуть страницу создания продажи. Пожалуйста, создайте продажу или отмените операцию.",
                                "OK",
                                "Отменить");
                            
                            // Если пользователь выбрал "Отменить", вызываем команду отмены
                            if (!result)
                            {
                                await createSalePage.ViewModel.CancelCommand.ExecuteAsync(null);
                            }
                            else
                            {
                                // Возвращаемся в раздел продаж
                                await Shell.Current.GoToAsync("///SalesListPage/CreateSalePage");
                            }
                        }
                    }
                }
                
                // Если мы перешли в раздел, отличный от продаж, закрываем страницу создания продажи (если навигация разрешена)
                var currentLocation2 = CurrentState?.Location?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(currentLocation2))
                {
                    // Проверяем, находимся ли мы в разделе продаж
                    bool isInSalesSection = currentLocation2.Contains("SalesListPage", StringComparison.OrdinalIgnoreCase) ||
                                            currentLocation2.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase);
                    
                    // Если мы не в разделе продаж, но в маршруте был CreateSalePage, закрываем её
                    if (!isInSalesSection && e.Previous?.Location?.ToString().Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Небольшая задержка для завершения навигации
                        await Task.Delay(100);
                        
                        // Проверяем, есть ли еще CreateSalePage в стеке навигации
                        var navigationStack = Current.Navigation.NavigationStack;
                        if (navigationStack != null)
                        {
                            foreach (var page in navigationStack)
                            {
                                if (page is CreateSalePage)
                                {
                                    // Закрываем страницу создания продажи
                                    await Current.GoToAsync("..");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
        }
    }
}
