// <copyright file="NotNullToVisibilityConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FatturaFacile.Wpf.Converters;

/// <summary>
/// Converts a non-null, non-empty object value to <see cref="Visibility.Visible"/>,
/// and null or empty strings to <see cref="Visibility.Collapsed"/>.
/// </summary>
public class NotNullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null && !string.IsNullOrWhiteSpace(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
