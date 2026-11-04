using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FatturaFacile.Desktop.Wpf.Converters;

/// <summary>
/// Converts boolean values to UI Element Visibility.
/// </summary>
public sealed class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolVal = value is true;
        if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
        {
            boolVal = !boolVal;
        }
        return boolVal ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}
