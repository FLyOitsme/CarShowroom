using System.Globalization;
using Microsoft.Maui.Controls;

namespace CarShowroom
{
    public class DiscountDescriptionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string description || string.IsNullOrWhiteSpace(description))
                return "Без условий";

            var conditions = new List<string>();
            var parts = description.ToLower().Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                
                // Цена
                if (trimmed.StartsWith("цена>") || trimmed.StartsWith("цена >"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Цена от {val:N0} ₽");
                }
                else if (trimmed.StartsWith("цена<") || trimmed.StartsWith("цена <"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Цена до {val:N0} ₽");
                }
                
                // Год
                else if (trimmed.StartsWith("год>") || trimmed.StartsWith("год >"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Год от {val:F0}");
                }
                else if (trimmed.StartsWith("год<") || trimmed.StartsWith("год <"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Год до {val:F0}");
                }
                
                // Тип
                else if (trimmed.StartsWith("тип=") || trimmed.StartsWith("тип ="))
                {
                    var typeName = ExtractValue(trimmed);
                    if (!string.IsNullOrEmpty(typeName))
                        conditions.Add($"Тип: {typeName}");
                }
                
                // Бренд
                else if (trimmed.StartsWith("бренд=") || trimmed.StartsWith("бренд ="))
                {
                    var brandName = ExtractValue(trimmed);
                    if (!string.IsNullOrEmpty(brandName))
                        conditions.Add($"Бренд: {brandName}");
                }
                
                // Состояние
                else if (trimmed.StartsWith("состояние=") || trimmed.StartsWith("состояние ="))
                {
                    var conditionName = ExtractValue(trimmed);
                    if (!string.IsNullOrEmpty(conditionName))
                        conditions.Add($"Состояние: {conditionName}");
                }
                
                // Пробег
                else if (trimmed.StartsWith("пробег>") || trimmed.StartsWith("пробег >"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Пробег от {val:N0} км");
                }
                else if (trimmed.StartsWith("пробег<") || trimmed.StartsWith("пробег <"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Пробег до {val:N0} км");
                }
                
                // Клиент
                else if (trimmed.Contains("первый_клиент") || trimmed.Contains("первый клиент") || trimmed.Contains("первая покупка"))
                {
                    conditions.Add("Первый клиент");
                }
                else if (trimmed.Contains("vip_клиент") || trimmed.Contains("vip клиент") || trimmed.Contains("вип клиент"))
                {
                    conditions.Add("VIP клиент");
                }
                
                // Покупки
                else if (trimmed.StartsWith("покупок>=") || trimmed.StartsWith("покупок >="))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"От {val:F0} покупок");
                }
                else if (trimmed.StartsWith("покупок<") || trimmed.StartsWith("покупок <"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"До {val:F0} покупок");
                }
            }

            return conditions.Any() ? string.Join(", ", conditions) : "Без условий";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool TryParseValue(string condition, out float value)
        {
            value = 0;
            int operatorIndex = -1;
            if (condition.Contains(">="))
                operatorIndex = condition.IndexOf(">=");
            else if (condition.Contains("<="))
                operatorIndex = condition.IndexOf("<=");
            else if (condition.Contains(">"))
                operatorIndex = condition.IndexOf(">");
            else if (condition.Contains("<"))
                operatorIndex = condition.IndexOf("<");
            else if (condition.Contains("="))
                operatorIndex = condition.IndexOf("=");
            
            if (operatorIndex >= 0 && operatorIndex < condition.Length - 1)
            {
                var valuePart = condition.Substring(operatorIndex + 1).Trim();
                valuePart = valuePart.TrimStart('=', '>', '<');
                if (float.TryParse(valuePart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float parsedValue))
                {
                    value = parsedValue;
                    return true;
                }
            }
            return false;
        }

        private string ExtractValue(string condition)
        {
            var index = condition.IndexOf('=');
            if (index >= 0 && index < condition.Length - 1)
            {
                return condition.Substring(index + 1).Trim();
            }
            return string.Empty;
        }
    }
}
