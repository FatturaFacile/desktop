// <copyright file="DeliveryEntryLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Procurement;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One editable line of the delivery notice of a dropshipment. An empty quantity means the line is not part of this delivery.
/// </summary>
/// <param name="lineId">The order line.</param>
/// <param name="label">The line number and description.</param>
/// <param name="openText">The open quantity with its unit.</param>
internal sealed partial class DeliveryEntryLine(Guid lineId, string label, string openText) : ObservableObject
{
    [ObservableProperty]
    private string _quantityText = string.Empty;

    /// <summary>
    /// Gets the order line.
    /// </summary>
    public Guid LineId { get; } = lineId;

    /// <summary>
    /// Gets the line number and description.
    /// </summary>
    public string Label { get; } = label;

    /// <summary>
    /// Gets the open quantity with its unit.
    /// </summary>
    public string OpenText { get; } = openText;

    /// <summary>
    /// Builds the request line.
    /// </summary>
    /// <param name="culture">The culture of the user.</param>
    /// <param name="line">The line, null when no quantity was typed.</param>
    /// <returns>False when the quantity is not a number.</returns>
    internal bool TryBuild(CultureInfo culture, out DeliveryLineRequest? line)
    {
        line = null;

        if (!ProcurementInput.TryDecimal(QuantityText, culture, out var quantity))
        {
            return false;
        }

        if (quantity is { } delivered)
        {
            line = new DeliveryLineRequest(LineId, delivered);
        }

        return true;
    }
}
