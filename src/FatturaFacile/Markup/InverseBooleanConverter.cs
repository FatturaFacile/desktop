using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Markup;

/// <summary>
/// Represents inverse boolean converter.
/// </summary>
public sealed class InverseBooleanConverter : IValueConverter
{
/// <summary>
/// Convert.
/// </summary>
/// <param name="value">The value.</param>
/// <param name="targetType">The target type.</param>
/// <param name="parameter">The parameter.</param>
/// <param name="culture">The culture.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

/// <summary>
/// Convert Back.
/// </summary>
/// <param name="value">The value.</param>
/// <param name="targetType">The target type.</param>
/// <param name="parameter">The parameter.</param>
/// <param name="culture">The culture.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}
