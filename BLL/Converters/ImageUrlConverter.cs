using System.Globalization;
using Microsoft.Maui.Controls;

namespace CarShowroom.Converters
{
    public class ImageUrlConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    return ImageSource.FromUri(new Uri(url));
                }
                catch
                {
                    return "dotnet_bot.png";
                }
            }
            return "dotnet_bot.png";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

