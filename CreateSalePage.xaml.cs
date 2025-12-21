using CarShowroom.ViewModels;
using Dom;
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
        
        public static CreateSalePage? CurrentInstance { get; private set; }

        public CreateSalePage(CreateSalePageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
            
            ViewModel.DiscountsAutoApplied += OnDiscountsAutoApplied;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            CurrentInstance = this;
            
            Shell.Current.Navigated += OnShellNavigated;
            
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
            if (ViewModel.IsNavigationLocked && !ViewModel.IsCancelling)
            {
                var target = e.Target?.Location?.ToString() ?? string.Empty;
                
                if (target.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                
                e.Cancel();
                
                var result = await Shell.Current.DisplayAlert(
                    "Внимание",
                    "Вы не можете покинуть страницу создания продажи. Пожалуйста, создайте продажу или отмените операцию.",
                    "OK",
                    "Отменить");
                
                if (!result)
                {
                    await ViewModel.CancelCommand.ExecuteAsync(null);
                }
            }
        }
        
        protected override bool OnBackButtonPressed()
        {
            if (ViewModel.IsNavigationLocked && !ViewModel.IsCancelling)
            {
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
                
                return true;
            }
            
            return base.OnBackButtonPressed();
        }
        
        private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            try
            {
                var currentLocation = Shell.Current.CurrentState?.Location?.ToString() ?? string.Empty;
                
                if (!string.IsNullOrEmpty(currentLocation) && 
                    !currentLocation.Contains("SalesListPage", StringComparison.OrdinalIgnoreCase) &&
                    !currentLocation.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                {
                    Shell.Current.Navigated -= OnShellNavigated;
                    
                    await Task.Delay(50);
                    
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch
            {
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            if (CurrentInstance == this)
            {
                CurrentInstance = null;
            }
            
            Shell.Current.Navigated -= OnShellNavigated;
            Shell.Current.Navigating -= OnShellNavigating;
            
            if (ViewModel != null)
            {
                ViewModel.DiscountsAutoApplied -= OnDiscountsAutoApplied;
            }
            
            try
            {
                if (ViewModel != null)
                {
                    ViewModel.ClearData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при очистке данных в OnDisappearing: {ex.Message}");
            }
            finally
            {
                if (ViewModel != null)
                {
                    ViewModel.IsCancelling = false;
                }
            }
        }

        private void OnDiscountsAutoApplied()
        {
            if (ViewModel == null) return;
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
                if (e.CurrentSelection?.FirstOrDefault() is Discount selectedDiscount)
                {
                    ViewModel.SelectedDiscount = selectedDiscount;
                }
                else if (e.CurrentSelection?.Count == 0)
                {
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
