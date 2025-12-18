using System.Globalization;
using Microsoft.Maui.Controls;
using DataLayer.Entities;

namespace CarShowroom
{
    public class DiscountSelectedConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values != null && values.Length >= 2)
            {
                var selectedDiscount = values[0] as Discount;
                var currentDiscount = values[1] as Discount;
                
                if (selectedDiscount != null && currentDiscount != null)
                {
                    return selectedDiscount.Id == currentDiscount.Id;
                }
            }
            return false;
        }

        public object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
