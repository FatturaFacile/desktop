// <copyright file="GridHeightConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace FatturaFacile.Converters;

/// <summary>
/// Multi-value converter and markup extension calculating the remaining grid height from total height and offset panels.
/// </summary>
public class GridHeightConverter : MarkupExtension, IMultiValueConverter
{
    /// <inheritdoc />
    public override object ProvideValue(
        IServiceProvider serviceProvider) => this;

    /// <summary>
    /// Gets or sets the margin distance subtracted from the summary calculation.
    /// </summary>
    public double SummaryDistance { get; set; }

    /// <inheritdoc />
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        var commonHeight = (double)values[0];
        var panelHeight = (double)values[1];
        var height = commonHeight - panelHeight - SummaryDistance;
        return height < 0 ? 0 : height;
    }

    /// <inheritdoc />
    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture) => throw new NotImplementedException();
}
