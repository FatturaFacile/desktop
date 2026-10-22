// <copyright file="HalfValueConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Data;

namespace FatturaFacile.Converters;

/// <summary>
/// Converts a numeric dimension to half its original value, optionally negating it.
/// </summary>
public class HalfValueConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets a value indicating whether to invert the sign of the halved result.
    /// </summary>
    public bool NegativeValue { get; set; }

    /// <inheritdoc />
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
    {
        if (!(value is double))
        {
            return value;
        }

        var result = ((double)value) / 2;
        if (NegativeValue)
        {
            result *= -1;
        }

        return result;
    }

    /// <inheritdoc />
    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}
