// <copyright file="InverseBooleanConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;

namespace FatturaFacile.Markup;

/// <summary>
/// Value converter that logically inverts a boolean value.
/// </summary>
public sealed class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// Inverts the supplied boolean value.
    /// </summary>
    /// <param name="value">The source boolean value to invert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The negated boolean value if <paramref name="value"/> is a boolean; otherwise, <see langword="false"/>.</returns>
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) => value is bool b && !b;

    /// <summary>
    /// Inverts the target boolean value back to the source boolean.
    /// </summary>
    /// <param name="value">The target boolean value to invert back.</param>
    /// <param name="targetType">The type of the binding source property.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The negated boolean value if <paramref name="value"/> is a boolean; otherwise, <see langword="false"/>.</returns>
    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) => value is bool b && !b;
}
