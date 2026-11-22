// <copyright file="AbcRunRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Display row of an ABC analysis run in the run history.
/// </summary>
internal sealed class AbcRunRow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbcRunRow"/> class.
    /// </summary>
    /// <param name="run">The run.</param>
    public AbcRunRow(AbcRunDto run)
    {
        ArgumentNullException.ThrowIfNull(run);

        Id = run.Id;
        Period = (run.PeriodStart ?? string.Empty) + " - " + (run.PeriodEnd ?? string.Empty);
        PeriodDays = run.PeriodDays;
        TotalConsumptionText = CountingText.Amount(run.TotalConsumptionValue);
        ClassCounts = string.Format(CultureInfo.InvariantCulture, "{0} / {1} / {2}", run.ClassACount, run.ClassBCount, run.ClassCCount);
        ItemCount = run.ItemCount;
        Applied = run.Applied;
    }

    /// <summary>
    /// Gets the run id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the analysed period as text.
    /// </summary>
    public string Period { get; }

    /// <summary>
    /// Gets the length of the period in days.
    /// </summary>
    public int PeriodDays { get; }

    /// <summary>
    /// Gets the formatted total consumption value.
    /// </summary>
    public string TotalConsumptionText { get; }

    /// <summary>
    /// Gets the number of items of class A, B and C as text.
    /// </summary>
    public string ClassCounts { get; }

    /// <summary>
    /// Gets the number of ranked items.
    /// </summary>
    public int ItemCount { get; }

    /// <summary>
    /// Gets a value indicating whether the classes were applied to the items.
    /// </summary>
    public bool Applied { get; }
}
