// <copyright file="ProcurementDisplay.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Procurement;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Display helpers of the purchase order fulfilment: the dual unit of measure.
/// </summary>
internal static class ProcurementDisplay
{
    private const string QuantityFormat = "0.####";

    /// <summary>
    /// Formats the ordered quantity of a line in base units with its conversion, for example "120 PZ (1 BOX = 12 PZ)".
    /// </summary>
    /// <param name="line">The line.</param>
    /// <param name="format">The localized format: base quantity {0}, base unit {1}, purchase unit {2}, factor {3}.</param>
    /// <param name="culture">The culture of the user.</param>
    /// <returns>The text; just the base quantity when both units are the same.</returns>
    internal static string BaseQuantity(PurchaseOrderLineView line, string format, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(line);

        var baseQuantity = line.QuantityBaseOrdered != 0m ? line.QuantityBaseOrdered : line.QuantityOrdered * line.UomConversionFactor;
        var quantityText = baseQuantity.ToString(QuantityFormat, culture);
        var baseUom = string.IsNullOrWhiteSpace(line.BaseUom) ? line.Uom : line.BaseUom;

        return line.UomConversionFactor == 1m && string.Equals(baseUom, line.Uom, StringComparison.Ordinal)
            ? (quantityText + " " + baseUom).Trim()
            : string.Format(culture, format, quantityText, baseUom, line.Uom, line.UomConversionFactor.ToString(QuantityFormat, culture));
    }
}
