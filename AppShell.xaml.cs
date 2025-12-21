namespace CarShowroom
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Routing.RegisterRoute("MainPage/CarDetailsPage", typeof(CarDetailsPage));
            Routing.RegisterRoute("MainPage/AddEditCarPage", typeof(AddEditCarPage));
            Routing.RegisterRoute("SalesListPage/CreateSalePage", typeof(CreateSalePage));
            Routing.RegisterRoute("AddEditDiscountPage", typeof(AddEditDiscountPage));
            Routing.RegisterRoute("ContractPreviewPage", typeof(ContractPreviewPage));
            
            Navigated += OnShellNavigated;
            
            Navigating += OnShellNavigating;
            
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
                if (_isNavigationLocked)
                {
                    var target = e.Target?.Location?.ToString() ?? string.Empty;
                    
                    if (target.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    
                    e.Cancel();
                    
                    if (CreateSalePage.CurrentInstance != null)
                    {
                        var createSalePage = CreateSalePage.CurrentInstance;
                        
                        var result = await Shell.Current.DisplayAlert(
                            "Внимание",
                            "Вы не можете покинуть страницу создания продажи. Пожалуйста, создайте продажу или отмените операцию.",
                            "OK",
                            "Отменить");
                        
                        if (!result)
                        {
                            await createSalePage.ViewModel.CancelCommand.ExecuteAsync(null);
                        }
                    }
                }
            }
            catch
            {
            }
        }
        
        private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            try
            {
                if (CreateSalePage.CurrentInstance != null)
                {
                    var createSalePage = CreateSalePage.CurrentInstance;
                    
                    if (createSalePage.ViewModel.IsNavigationLocked && !createSalePage.ViewModel.IsCancelling)
                    {
                        var currentLocation = CurrentState?.Location?.ToString() ?? string.Empty;
                        
                        if (!string.IsNullOrEmpty(currentLocation) && 
                            !currentLocation.Contains("SalesListPage", StringComparison.OrdinalIgnoreCase) &&
                            !currentLocation.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                        {
                            var result = await Shell.Current.DisplayAlert(
                                "Внимание",
                                "Вы не можете покинуть страницу создания продажи. Пожалуйста, создайте продажу или отмените операцию.",
                                "OK",
                                "Отменить");
                            
                            if (!result)
                            {
                                await createSalePage.ViewModel.CancelCommand.ExecuteAsync(null);
                            }
                            else
                            {
                                await Shell.Current.GoToAsync("///SalesListPage/CreateSalePage");
                            }
                        }
                    }
                }
                
                var currentLocation2 = CurrentState?.Location?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(currentLocation2))
                {
                    bool isInSalesSection = currentLocation2.Contains("SalesListPage", StringComparison.OrdinalIgnoreCase) ||
                                            currentLocation2.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase);
                    
                    if (!isInSalesSection && e.Previous?.Location?.ToString().Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        await Task.Delay(100);
                        
                        var navigationStack = Current.Navigation.NavigationStack;
                        if (navigationStack != null)
                        {
                            foreach (var page in navigationStack)
                            {
                                if (page is CreateSalePage)
                                {
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
            }
        }
    }
}
