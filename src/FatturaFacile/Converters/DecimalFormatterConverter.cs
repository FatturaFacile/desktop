// <copyright file="DecimalFormatterConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Wpf.Converters;

/// <summary>
/// Two-way value converter formatting decimal numbers to local currency/number standard notation ("N2") and parsing them back.
/// </summary>
public class DecimalFormatterConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is decimal d ? d.ToString("N2", CultureInfo.CurrentCulture) : "0.00";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out var d))
        {
            return d;
        }

        return 0m;
    }
}
