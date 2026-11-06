// <copyright file="MasterDataLookups.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Values offered by the master data combo boxes.
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class MasterDataLookups
{
    /// <summary>
    /// Gets the contact types.
    /// </summary>
    public static IReadOnlyList<string> ContactTypes { get; } = ["client", "supplier", "both"];

    /// <summary>
    /// Gets the most common countries.
    /// </summary>
    public static IReadOnlyList<string> Countries { get; } = ["CH", "IT", "LI", "DE", "FR", "AT", "US", "GB"];

    /// <summary>
    /// Gets the currencies.
    /// </summary>
    public static IReadOnlyList<string> Currencies { get; } = ["CHF", "EUR", "USD", "GBP"];

    /// <summary>
    /// Gets the depreciation methods.
    /// </summary>
    public static IReadOnlyList<string> DepreciationMethods { get; } = ["StraightLine", "Declining"];
}
