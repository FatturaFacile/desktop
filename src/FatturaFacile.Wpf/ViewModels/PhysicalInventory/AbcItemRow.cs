// <copyright file="AbcItemRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Display row of one ranked item of an ABC analysis run.
/// </summary>
internal sealed class AbcItemRow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbcItemRow"/> class.
    /// </summary>
    /// <param name="item">The ranked item.</param>
    /// <param name="localization">The localization service.</param>
    public AbcItemRow(AbcRunItemDto item, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(localization);

        StockItemId = item.StockItemId;
        Rank = item.Rank;
        Sku = item.Sku;
        Name = item.Name;
        AbcClass = item.AbcClass;
        PreviousClass = item.PreviousClass ?? string.Empty;
        ConsumptionText = CountingText.Amount(item.ConsumptionValue);
        CumulativeText = item.CumulativeShare.ToString("0.00", CultureInfo.CurrentCulture) + " %";
        IsManualOverride = item.IsManualOverride;
        IsNewItem = item.IsNewItem;

        List<string> flags = [];
        if (item.IsManualOverride)
        {
            flags.Add(localization["Inv_Abc_Manual"]);
        }

        if (item.IsNewItem)
        {
            flags.Add(localization["Inv_Abc_NewItem"]);
        }

        Flags = string.Join(", ", flags);
    }

    /// <summary>
    /// Gets the stock item id.
    /// </summary>
    public string StockItemId { get; }

    /// <summary>
    /// Gets the rank by consumption value.
    /// </summary>
    public int Rank { get; }

    /// <summary>
    /// Gets the SKU.
    /// </summary>
    public string Sku { get; }

    /// <summary>
    /// Gets the item name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the class (A, B or C).
    /// </summary>
    public string AbcClass { get; }

    /// <summary>
    /// Gets the class before the run, or an empty text.
    /// </summary>
    public string PreviousClass { get; }

    /// <summary>
    /// Gets the formatted consumption value.
    /// </summary>
    public string ConsumptionText { get; }

    /// <summary>
    /// Gets the formatted cumulative percentage.
    /// </summary>
    public string CumulativeText { get; }

    /// <summary>
    /// Gets a value indicating whether a manual override fixes the class.
    /// </summary>
    public bool IsManualOverride { get; }

    /// <summary>
    /// Gets a value indicating whether the item is new (no consumption history).
    /// </summary>
    public bool IsNewItem { get; }

    /// <summary>
    /// Gets the localized flags (manual override, new item).
    /// </summary>
    public string Flags { get; }
}
