// <copyright file="MaintenanceCoverageWire.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Maps <see cref="MaintenanceCoverageType"/> to and from the `coverage_type` strings of the web-api.
/// </summary>
public static class MaintenanceCoverageWire
{
    private static readonly Dictionary<MaintenanceCoverageType, string> Names = new()
    {
        [MaintenanceCoverageType.LaborOnly] = "labor_only",
        [MaintenanceCoverageType.PartsAndLabor] = "parts_and_labor",
        [MaintenanceCoverageType.PreventiveOnly] = "preventive_only",
        [MaintenanceCoverageType.FullComprehensive] = "full_comprehensive",
    };

    /// <summary>
    /// Gets the wire name of a coverage type.
    /// </summary>
    /// <param name="coverage">The coverage type.</param>
    /// <returns>The `coverage_type` string.</returns>
    public static string ToWire(MaintenanceCoverageType coverage) => Names.GetValueOrDefault(coverage, "labor_only");

    /// <summary>
    /// Reads a wire name; an unknown or missing value is read as <see cref="MaintenanceCoverageType.LaborOnly"/>, the narrowest
    /// coverage, so a contract is never shown as covering more than the server says.
    /// </summary>
    /// <param name="wire">The `coverage_type` string.</param>
    /// <returns>The coverage type.</returns>
    public static MaintenanceCoverageType FromWire(string? wire) =>
        Names.FirstOrDefault(pair => string.Equals(pair.Value, wire, StringComparison.Ordinal)).Key;
}
