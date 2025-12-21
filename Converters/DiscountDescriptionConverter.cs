using System.Globalization;
using System.Linq;
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
            // Используем точку с запятой для разделения основных условий (новый формат)
            // Если точка с запятой не найдена, используем запятую для обратной совместимости (старый формат)
            var parts = description.Contains(';')
                ? description.Split(new[] { ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                : description.ToLower().Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                var trimmedLower = trimmed.ToLower();
                
                // Цена
                if (trimmedLower.StartsWith("цена>") || trimmedLower.StartsWith("цена >"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Цена от {val:N0} ₽");
                }
                else if (trimmedLower.StartsWith("цена<") || trimmedLower.StartsWith("цена <"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Цена до {val:N0} ₽");
                }
                
                // Год
                else if (trimmedLower.StartsWith("год>") || trimmedLower.StartsWith("год >"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Год от {val:F0}");
                }
                else if (trimmedLower.StartsWith("год<") || trimmedLower.StartsWith("год <"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Год до {val:F0}");
                }
                
                // Тип (поддерживаем множественные значения через запятую)
                else if (trimmedLower.StartsWith("тип=") || trimmedLower.StartsWith("тип ="))
                {
                    var typeNames = ExtractValue(trimmed).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t));
                    if (typeNames.Any())
                        conditions.Add($"Тип: {string.Join(", ", typeNames)}");
                }
                
                // Бренд (поддерживаем множественные значения через запятую)
                else if (trimmedLower.StartsWith("бренд=") || trimmedLower.StartsWith("бренд ="))
                {
                    var brandNames = ExtractValue(trimmed).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(b => b.Trim())
                        .Where(b => !string.IsNullOrEmpty(b));
                    if (brandNames.Any())
                        conditions.Add($"Бренд: {string.Join(", ", brandNames)}");
                }
                
                // Состояние (поддерживаем множественные значения через запятую)
                else if (trimmedLower.StartsWith("состояние=") || trimmedLower.StartsWith("состояние ="))
                {
                    var conditionNames = ExtractValue(trimmed).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c));
                    if (conditionNames.Any())
                        conditions.Add($"Состояние: {string.Join(", ", conditionNames)}");
                }
                
                // Пробег
                else if (trimmedLower.StartsWith("пробег>") || trimmedLower.StartsWith("пробег >"))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"Пробег от {val:N0} км");
                }
                else if (trimmedLower.StartsWith("пробег<") || trimmedLower.StartsWith("пробег <"))
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
                else if (trimmedLower.StartsWith("покупок>=") || trimmedLower.StartsWith("покупок >="))
                {
                    if (TryParseValue(trimmed, out float val))
                        conditions.Add($"От {val:F0} покупок");
                }
                else if (trimmedLower.StartsWith("покупок<") || trimmedLower.StartsWith("покупок <"))
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
