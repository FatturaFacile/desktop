// <copyright file="InverseBooleanToVisibilityConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FatturaFacile.Wpf.Converters;

/// <summary>
/// Converts <see langword="true"/> to <see cref="Visibility.Collapsed"/> and <see langword="false"/> to <see cref="Visibility.Visible"/>.
/// </summary>
[SuppressMessage("Design", "MA0182:Unused internal type", Justification = "Used from XAML.")]
internal sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not Visibility.Visible;
}
