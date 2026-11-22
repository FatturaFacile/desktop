// <copyright file="CyclePlanRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Display row of a cycle count plan.
/// </summary>
internal sealed class CyclePlanRow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CyclePlanRow"/> class.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="localization">The localization service.</param>
    public CyclePlanRow(CycleCountPlanDto plan, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(localization);

        Id = plan.Id;
        Name = plan.Name;
        WarehouseId = plan.WarehouseId;
        ZoneId = plan.WarehouseZoneId ?? string.Empty;
        AbcClass = plan.AbcClass ?? string.Empty;
        CadenceLabel = CountingText.CadenceLabel(plan.Cadence, localization);
        IntervalDays = plan.IntervalDays;
        FreezesStock = plan.FreezesStock;
        IsActive = plan.IsActive;
        LastRunAt = plan.LastRunAt ?? string.Empty;
        NextRunOn = plan.NextRunOn ?? string.Empty;
    }

    /// <summary>
    /// Gets the plan id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the plan name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the warehouse id.
    /// </summary>
    public string WarehouseId { get; }

    /// <summary>
    /// Gets the zone id, or an empty text.
    /// </summary>
    public string ZoneId { get; }

    /// <summary>
    /// Gets the ABC class counted by the plan, or an empty text.
    /// </summary>
    public string AbcClass { get; }

    /// <summary>
    /// Gets the localized cadence.
    /// </summary>
    public string CadenceLabel { get; }

    /// <summary>
    /// Gets the interval in days.
    /// </summary>
    public int IntervalDays { get; }

    /// <summary>
    /// Gets a value indicating whether the counts freeze the stock.
    /// </summary>
    public bool FreezesStock { get; }

    /// <summary>
    /// Gets a value indicating whether the plan is active.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Gets when the plan last ran, or an empty text.
    /// </summary>
    public string LastRunAt { get; }

    /// <summary>
    /// Gets the next due date, or an empty text.
    /// </summary>
    public string NextRunOn { get; }
}
