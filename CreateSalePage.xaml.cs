using CarShowroom.ViewModels;
using Microsoft.Maui.Controls;

namespace CarShowroom
{
    [QueryProperty(nameof(CarId), "carId")]
    public partial class CreateSalePage : ContentPage
    {
        public CreateSalePageViewModel ViewModel { get; }

        public string CarId { get; set; } = string.Empty;

        public CreateSalePage(CreateSalePageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            long? carId = null;
            if (!string.IsNullOrEmpty(CarId) && long.TryParse(CarId, out long id))
            {
                carId = id;
            }
            ViewModel.Initialize(carId);
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
                ViewModel.OnDiscountSelectionChanged(collectionView.SelectedItems);
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
