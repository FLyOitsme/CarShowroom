using CarShowroom.ViewModels;

namespace CarShowroom
{
    [QueryProperty(nameof(DiscountId), "discountId")]
    public partial class AddEditDiscountPage : ContentPage
    {
        public AddEditDiscountPageViewModel ViewModel { get; }

        public string DiscountId { get; set; } = string.Empty;

        public AddEditDiscountPage(AddEditDiscountPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            int? discountId = null;
            if (!string.IsNullOrEmpty(DiscountId) && int.TryParse(DiscountId, out int id))
            {
                discountId = id;
            }
            ViewModel.Initialize(discountId);
        }
    }
}
