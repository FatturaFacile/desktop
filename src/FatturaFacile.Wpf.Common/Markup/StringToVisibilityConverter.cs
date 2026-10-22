// <copyright file="StringToVisibilityConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FatturaFacile.Markup;

/// <summary>
/// Represents string to visibility converter.
/// </summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Convert.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) => value is string s && !string.IsNullOrWhiteSpace(s)
        ? Visibility.Visible
        : Visibility.Collapsed;

    /// <summary>
    /// Convert Back.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    /// <exception cref="NotSupportedException">The operation is not supported.</exception>
    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}
