using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarShowroom.Interfaces;
using DataLayer.Entities;

namespace CarShowroom.ViewModels
{
    public partial class DiscountsListPageViewModel : ObservableObject
    {
        private readonly ISaleService _saleService;

        [ObservableProperty]
        private List<Discount> _discounts = new();

        [ObservableProperty]
        private Discount? _selectedDiscount;

        public DiscountsListPageViewModel(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [RelayCommand]
        private async Task LoadDiscountsAsync()
        {
            try
            {
                Discounts = await _saleService.GetAllDiscountsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить акции: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task DiscountSelectedAsync(Discount? discount)
        {
            if (discount != null)
            {
                var action = await Shell.Current.DisplayActionSheet(
                    "Выберите действие",
                    "Отмена",
                    "Удалить",
                    "Редактировать");
                
                if (action == "Редактировать")
                {
                    await EditDiscountAsync(discount);
                }
                else if (action == "Удалить")
                {
                    await DeleteDiscountAsync(discount);
                }
                
                SelectedDiscount = null;
            }
        }

        [RelayCommand]
        private async Task AddDiscountAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddEditDiscountPage));
        }

        [RelayCommand]
        private async Task EditDiscountAsync(Discount discount)
        {
            await Shell.Current.GoToAsync($"{nameof(AddEditDiscountPage)}?discountId={discount.Id}");
        }

        private async Task DeleteDiscountAsync(Discount discount)
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Подтверждение",
                $"Вы уверены, что хотите удалить акцию \"{discount.Name}\"?",
                "Да",
                "Нет");

            if (confirm)
            {
                try
                {
                    await _saleService.DeleteDiscountAsync(discount.Id);
                    await Shell.Current.DisplayAlert("Успех", "Акция удалена", "OK");
                    await LoadDiscountsAsync();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Ошибка", $"Не удалось удалить акцию: {ex.Message}", "OK");
                }
            }
        }
    }
}
