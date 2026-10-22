// <copyright file="CategoryColorConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace FatturaFacile.Wpf.Common.Converters;

/// <summary>
/// Converts a hexadecimal color string (e.g. #FF5500) into a WPF <see cref="Color"/>.
/// </summary>
public class CategoryColorConverter : IValueConverter
{

    /// <inheritdoc />
    public object Convert(
        object?     value,
        Type        targetType,
        object?     parameter,
        CultureInfo culture)
    {
        if (value == null)
        {
            return Colors.Transparent;
        }

        var str = value.ToString();

        return (str != null && Regex.IsMatch(input: str, pattern: @"^#[0-9A-Fa-f]+"))
                   ? (Color)ColorConverter.ConvertFromString(value: str)
                   : Colors.Transparent;
    }

    /// <inheritdoc />
    public object? ConvertBack(
        object?     value,
        Type        targetType,
        object?     parameter,
        CultureInfo culture) => value?.ToString();

}
