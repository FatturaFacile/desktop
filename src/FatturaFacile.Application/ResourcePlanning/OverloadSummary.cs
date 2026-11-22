// <copyright file="OverloadSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Whether a mutation overloads the employee and in which weeks.
/// </summary>
/// <param name="IsOverloaded">The is overloaded.</param>
/// <param name="PeakLoadPercent">The peak load percent.</param>
/// <param name="Weeks">The weeks with their load.</param>
public sealed record OverloadSummary(
    bool IsOverloaded,
    double PeakLoadPercent,
    IReadOnlyList<OverloadWeek> Weeks);
