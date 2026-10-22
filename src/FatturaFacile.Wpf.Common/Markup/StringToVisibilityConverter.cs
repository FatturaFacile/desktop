// <copyright file="StringToVisibilityConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FatturaFacile.Wpf.Common.Markup;

/// <summary>
/// Value converter that maps string presence or non-whitespace content to <see cref="Visibility.Visible"/>, and null or whitespace strings to <see cref="Visibility.Collapsed"/>.
/// </summary>
public sealed class StringToVisibilityConverter : IValueConverter
{

    /// <summary>
    /// Evaluates whether the string contains non-whitespace content and returns the corresponding visibility state.
    /// </summary>
    /// <param name="value">The source string to evaluate.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns><see cref="Visibility.Visible"/> if <paramref name="value"/> is a non-empty, non-whitespace string; otherwise, <see cref="Visibility.Collapsed"/>.</returns>
    public object Convert(
        object      value,
        Type        targetType,
        object      parameter,
        CultureInfo culture) => value is string s && !string.IsNullOrWhiteSpace(value: s)
                                    ? Visibility.Visible
                                    : Visibility.Collapsed;

    /// <summary>
    /// Reverse conversion is not supported for string-to-visibility mappings.
    /// </summary>
    /// <param name="value">The target visibility value.</param>
    /// <param name="targetType">The source property type.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>Never returns normally.</returns>
    /// <exception cref="NotSupportedException">Always thrown because reverse conversion is not supported.</exception>
    public object ConvertBack(
        object      value,
        Type        targetType,
        object      parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();

}
