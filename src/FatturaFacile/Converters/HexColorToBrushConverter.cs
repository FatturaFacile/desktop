// <copyright file="HexColorToBrushConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FatturaFacile.Wpf.Converters;

/// <summary>
/// Converts a hexadecimal color string (e.g. "#FFFFFF") to a WPF <see cref="SolidColorBrush"/>.
/// </summary>
public class HexColorToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }
            catch
            {
                // Fall back to transparent on invalid hex formats
            }
        }

        return Brushes.Transparent;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
