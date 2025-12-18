using CarShowroom.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

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
            
            // Подписываемся на событие автоматического применения скидок
            ViewModel.DiscountsAutoApplied += OnDiscountsAutoApplied;
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // При выходе со страницы вызываем метод отмены (если отмена не была вызвана явно)
            if (!ViewModel.IsCancelling)
            {
                // Вызываем метод отмены (без навигации, так как навигация происходит автоматически)
                ViewModel.CancelCommand.ExecuteAsync(null);
            }
            // Сбрасываем флаг после обработки
            ViewModel.IsCancelling = false;
        }

        private void OnDiscountsAutoApplied()
        {
            // Программно устанавливаем выбранную скидку в CollectionView
            // Используем BeginInvokeOnMainThread для гарантии, что UI готов
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (DiscountsCollectionView != null && ViewModel.SelectedDiscount != null)
                {
                    // Устанавливаем выбранную скидку
                    DiscountsCollectionView.SelectedItem = ViewModel.SelectedDiscount;
                }
            });
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
                ViewModel.OnDiscountSelectionChanged(sender, e);
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
