// <copyright file="VarianceLineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Display row of the reconciliation grid: system, counted and final quantities and the variance of one count line.
/// </summary>
internal sealed partial class VarianceLineRow : ObservableObject
{
    [ObservableProperty]
    private bool _isMarked;

    /// <summary>
    /// Initializes a new instance of the <see cref="VarianceLineRow"/> class.
    /// </summary>
    /// <param name="line">The variance line.</param>
    /// <param name="localization">The localization service.</param>
    public VarianceLineRow(CountVarianceLineDto line, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(localization);

        Id = line.Id;
        Bin = line.BinBarcode ?? line.WarehouseBinId ?? string.Empty;
        Sku = line.Sku;
        ItemName = line.ItemName;
        IsCounted = line.IsCounted;
        RecountRequired = line.RecountRequired;
        VarianceQuantity = line.VarianceQuantity;
        VarianceValue = line.VarianceValue;
        SystemQuantityText = CountingText.Quantity(line.SystemQuantity);
        CountedQuantityText = CountingText.Quantity(line.CountedQuantity);
        FinalQuantityText = CountingText.Quantity(line.FinalQuantity);
        VarianceQuantityText = line.IsCounted ? CountingText.SignedQuantity(line.VarianceQuantity) : string.Empty;
        VarianceValueText = line.IsCounted ? CountingText.SignedAmount(line.VarianceValue) : string.Empty;
        VariancePercentText = FormatPercent(line, localization);
        RecountText = line.RecountRequired
            ? localization["Inv_Recon_RecountPending"] + " (" + localization["Inv_Recon_Round"] + " " + line.RecountRound.ToString(CultureInfo.CurrentCulture) + ")"
            : string.Empty;
    }

    /// <summary>
    /// Gets the line id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the bin barcode (or id).
    /// </summary>
    public string Bin { get; }

    /// <summary>
    /// Gets the SKU.
    /// </summary>
    public string Sku { get; }

    /// <summary>
    /// Gets the item name.
    /// </summary>
    public string ItemName { get; }

    /// <summary>
    /// Gets a value indicating whether the line was counted.
    /// </summary>
    public bool IsCounted { get; }

    /// <summary>
    /// Gets a value indicating whether a recount is pending.
    /// </summary>
    public bool RecountRequired { get; }

    /// <summary>
    /// Gets the variance quantity (final minus system).
    /// </summary>
    public decimal VarianceQuantity { get; }

    /// <summary>
    /// Gets the variance value.
    /// </summary>
    public decimal VarianceValue { get; }

    /// <summary>
    /// Gets the formatted system quantity.
    /// </summary>
    public string SystemQuantityText { get; }

    /// <summary>
    /// Gets the formatted counted quantity, empty when not counted.
    /// </summary>
    public string CountedQuantityText { get; }

    /// <summary>
    /// Gets the formatted final quantity, empty when not available.
    /// </summary>
    public string FinalQuantityText { get; }

    /// <summary>
    /// Gets the formatted signed variance quantity, empty when not counted.
    /// </summary>
    public string VarianceQuantityText { get; }

    /// <summary>
    /// Gets the formatted signed variance value, empty when not counted.
    /// </summary>
    public string VarianceValueText { get; }

    /// <summary>
    /// Gets the variance percentage: the signed percentage, the localized "not available" text when it is undefined
    /// (system quantity is zero), or empty when the line was not counted.
    /// </summary>
    public string VariancePercentText { get; }

    /// <summary>
    /// Gets the recount state text (pending and round), empty when no recount is pending.
    /// </summary>
    public string RecountText { get; }

    /// <summary>
    /// Gets a value indicating whether the counted quantity differs from the system quantity.
    /// </summary>
    public bool HasVariance => IsCounted && VarianceQuantity != 0m;

    /// <summary>
    /// Gets a value indicating whether the variance is a gain (more stock than the system expected).
    /// </summary>
    public bool IsGain => HasVariance && VarianceQuantity > 0m;

    /// <summary>
    /// Gets a value indicating whether the variance is a loss (less stock than the system expected).
    /// </summary>
    public bool IsLoss => HasVariance && VarianceQuantity < 0m;

    /// <summary>
    /// Gets a value indicating whether a recount can be requested for the line (counted, no recount pending yet).
    /// </summary>
    public bool CanRecount => IsCounted && !RecountRequired;

    private static string FormatPercent(CountVarianceLineDto line, ILocalizationService localization)
    {
        if (line.VariancePercentageUndefined)
        {
            return localization["Inv_Recon_Undefined"];
        }

        return line.VariancePercentage is { } percent
            ? percent.ToString("+0.00;-0.00;0.00", CultureInfo.CurrentCulture) + " %"
            : string.Empty;
    }
}
