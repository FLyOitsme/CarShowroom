using CarShowroom.ViewModels;
using Microsoft.Maui.Controls;

namespace CarShowroom
{
    public partial class DiscountsListPage : ContentPage
    {
        public DiscountsListPageViewModel ViewModel { get; }

        public DiscountsListPage(DiscountsListPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.LoadDiscountsCommand.ExecuteAsync(null);
        }

        private async void OnDiscountSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is DataLayer.Entities.Discount selectedDiscount)
            {
                await ViewModel.DiscountSelectedCommand.ExecuteAsync(selectedDiscount);
            }
        }
    }
}
