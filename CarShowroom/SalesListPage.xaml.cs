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
            
            try
            {
                var currentLocation = Shell.Current.CurrentState?.Location?.ToString() ?? string.Empty;
                
                if (currentLocation.Contains("CreateSalePage", StringComparison.OrdinalIgnoreCase))
                {
                    await Task.Delay(150);
                    
                    try
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                    catch
                    {
                        await Shell.Current.GoToAsync("///SalesListPage");
                    }
                }
                
                var navigationStack = Shell.Current.Navigation.NavigationStack;
                if (navigationStack != null && navigationStack.Count > 0)
                {
                    var createSalePage = navigationStack.OfType<CreateSalePage>().FirstOrDefault();
                    if (createSalePage != null)
                    {
                        await Task.Delay(100);
                        await Shell.Current.GoToAsync("///SalesListPage");
                    }
                }
            }
            catch
            {
            }
            
            await ViewModel.LoadSalesCommand.ExecuteAsync(null);
        }

        private async void OnSaleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Dom.Sale selectedSale)
            {
                await ViewModel.SaleSelectedCommand.ExecuteAsync(selectedSale);
            }
        }
    }
}

