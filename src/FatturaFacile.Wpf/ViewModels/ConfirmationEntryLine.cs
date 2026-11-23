// <copyright file="ConfirmationEntryLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Procurement;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One editable line of the supplier confirmation being entered. An untouched line means the supplier confirms it as ordered.
/// </summary>
/// <param name="lineId">The order line.</param>
/// <param name="label">The line number and description.</param>
/// <param name="orderedText">The ordered quantity with its unit.</param>
/// <param name="orderedQuantity">The ordered quantity.</param>
/// <param name="orderedPrice">The ordered unit price.</param>
internal sealed partial class ConfirmationEntryLine(Guid lineId, string label, string orderedText, decimal orderedQuantity, decimal orderedPrice) : ObservableObject
{
    [ObservableProperty]
    private string _confirmedQuantityText = string.Empty;

    [ObservableProperty]
    private string _confirmedPriceText = string.Empty;

    [ObservableProperty]
    private DateTime? _confirmedDate;

    [ObservableProperty]
    private string _reason = string.Empty;

    /// <summary>
    /// Gets the order line.
    /// </summary>
    public Guid LineId { get; } = lineId;

    /// <summary>
    /// Gets the line number and description.
    /// </summary>
    public string Label { get; } = label;

    /// <summary>
    /// Gets the ordered quantity with its unit.
    /// </summary>
    public string OrderedText { get; } = orderedText;

    /// <summary>
    /// Gets the ordered quantity.
    /// </summary>
    public decimal OrderedQuantity { get; } = orderedQuantity;

    /// <summary>
    /// Gets the ordered unit price.
    /// </summary>
    public decimal OrderedPrice { get; } = orderedPrice;

    /// <summary>
    /// Builds the request line.
    /// </summary>
    /// <param name="culture">The culture of the user.</param>
    /// <param name="line">The line, null when the user left it untouched.</param>
    /// <returns>False when a number is not valid.</returns>
    internal bool TryBuild(CultureInfo culture, out ConfirmationLineRequest? line)
    {
        line = null;

        if (!ProcurementInput.TryDecimal(ConfirmedQuantityText, culture, out var quantity) || !ProcurementInput.TryDecimal(ConfirmedPriceText, culture, out var price))
        {
            return false;
        }

        var reason = string.IsNullOrWhiteSpace(Reason) ? null : Reason.Trim();
        if (quantity is null && price is null && ConfirmedDate is null && reason is null)
        {
            return true;
        }

        line = new ConfirmationLineRequest(
            LineId,
            quantity ?? OrderedQuantity,
            price ?? OrderedPrice,
            ConfirmedDate is { } date ? DateOnly.FromDateTime(date) : null,
            reason);

        return true;
    }
}
