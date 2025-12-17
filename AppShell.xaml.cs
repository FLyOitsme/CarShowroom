namespace CarShowroom
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Регистрация маршрутов для навигации
            Routing.RegisterRoute("CarDetailsPage", typeof(CarDetailsPage));
            Routing.RegisterRoute("AddEditCarPage", typeof(AddEditCarPage));
            Routing.RegisterRoute("CreateSalePage", typeof(CreateSalePage));
        }
    }
}
