using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Desktop.Wpf.Converters;

/// <summary>
/// Formats decimals according to Swiss currency rounding (CHF 0.05).
/// </summary>
public sealed class SwissCurrencyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            var currency = parameter as string ?? "CHF";
            return $"{currency} {amount:N2}";
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && decimal.TryParse(s.Replace("CHF", "").Trim(), out var result))
        {
            return result;
        }
        return 0m;
    }
}
