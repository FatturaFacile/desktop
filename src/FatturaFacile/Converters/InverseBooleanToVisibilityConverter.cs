// <copyright file="InverseBooleanToVisibilityConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FatturaFacile.Wpf.Converters;

/// <summary>
/// Converts an inverted boolean value to <see cref="Visibility.Collapsed"/> when true, and <see cref="Visibility.Visible"/> when false.
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not Visibility.Visible;
    }
}
