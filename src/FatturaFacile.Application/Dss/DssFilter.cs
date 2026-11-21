// <copyright file="DssFilter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

namespace FatturaFacile.Application.Dss;

/// <summary>
/// The strategic filter of the Executive DSS cockpit: the key under which a snapshot is cached and the query of the web-api.
/// </summary>
/// <param name="LegalEntityId">The server legal entity, or an empty string for every legal entity.</param>
/// <param name="Currency">The reporting currency.</param>
/// <param name="PeriodPreset">The period preset.</param>
/// <param name="ComparisonBaseline">The comparison baseline.</param>
public sealed record DssFilter(string LegalEntityId, string Currency, string PeriodPreset, string ComparisonBaseline)
{
    /// <summary>
    /// The reporting currencies.
    /// </summary>
    public static readonly IReadOnlyList<string> Currencies = new ReadOnlyCollection<string>(["CHF", "EUR"]);

    /// <summary>
    /// The period presets (a custom range is not cached offline).
    /// </summary>
    public static readonly IReadOnlyList<string> PeriodPresets = new ReadOnlyCollection<string>(["MTD", "QTD", "YTD", "L12M"]);

    /// <summary>
    /// The comparison baselines.
    /// </summary>
    public static readonly IReadOnlyList<string> Baselines = new ReadOnlyCollection<string>(["PRIOR_PERIOD", "PRIOR_YEAR", "BUDGET"]);

    /// <summary>
    /// Gets the default filter: every legal entity, CHF, year to date against the prior year.
    /// </summary>
    public static DssFilter Default { get; } = new(string.Empty, "CHF", "YTD", "PRIOR_YEAR");
}
