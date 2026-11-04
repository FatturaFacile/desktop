// <copyright file="SwissCurrencyConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Desktop.Wpf.Converters;

/// <summary>
/// Formats decimals according to Swiss currency rounding (CHF 0.05).
/// </summary>
internal sealed class SwissCurrencyConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            var currency = parameter as string ?? "CHF";
            return $"{currency} {amount.ToString("N2", CultureInfo.InvariantCulture)}";
        }

        return value?.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s && decimal.TryParse(s.Replace("CHF", string.Empty, StringComparison.Ordinal).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? result : 0m;
}
