using CarShowroom.ViewModels;

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

