// <copyright file="AnalyticalEntryLineDraft.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// One line of a manual analytical entry: a signed amount on a cost element, allocated to one dimension set or split over several
/// allocation vectors.
/// </summary>
/// <param name="Amount">The signed amount.</param>
/// <param name="CostElementType">The cost element type.</param>
/// <param name="CostCenterId">The cost center, when the line is not split.</param>
/// <param name="ProfitCenterId">The profit center, when the line is not split.</param>
/// <param name="Vectors">The allocation vectors of a split line.</param>
public sealed record AnalyticalEntryLineDraft(
    decimal Amount,
    string CostElementType,
    string? CostCenterId = null,
    string? ProfitCenterId = null,
    IReadOnlyList<AllocationVector>? Vectors = null);
