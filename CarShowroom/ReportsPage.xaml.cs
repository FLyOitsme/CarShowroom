using CarShowroom.ViewModels;

namespace CarShowroom
{
    public partial class ReportsPage : ContentPage
    {
        public ReportsPageViewModel ViewModel { get; }

        public ReportsPage(ReportsPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.LoadReportsCommand.ExecuteAsync(null);
        }
    }
}

