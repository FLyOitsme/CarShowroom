namespace CarShowroom
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Регистрация маршрутов для навигации
            Routing.RegisterRoute(nameof(CarDetailsPage), typeof(CarDetailsPage));
            Routing.RegisterRoute(nameof(AddEditCarPage), typeof(AddEditCarPage));
            Routing.RegisterRoute(nameof(SaleContractPage), typeof(SaleContractPage));
        }
    }
}
