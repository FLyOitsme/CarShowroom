using CarShowroom.ViewModels;

namespace CarShowroom
{
    [QueryProperty(nameof(SaleId), "saleId")]
    [QueryProperty(nameof(BasePrice), "basePrice")]
    [QueryProperty(nameof(FinalPrice), "finalPrice")]
    public partial class ContractPreviewPage : ContentPage
    {
        public ContractPreviewPageViewModel ViewModel { get; }

        public string SaleId { get; set; } = string.Empty;
        public string BasePrice { get; set; } = string.Empty;
        public string FinalPrice { get; set; } = string.Empty;

        public ContractPreviewPage(ContractPreviewPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            if (long.TryParse(SaleId, out long saleId) &&
                decimal.TryParse(BasePrice, out decimal basePrice) &&
                decimal.TryParse(FinalPrice, out decimal finalPrice))
            {
                ViewModel.Initialize(saleId, basePrice, finalPrice);
            }
        }
    }
}

