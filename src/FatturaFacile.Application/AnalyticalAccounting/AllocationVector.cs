// <copyright file="AllocationVector.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// One analytical allocation vector of a posting line: where a share of the line goes, by percentage or by exact amount.
/// </summary>
/// <param name="CostCenterId">The cost center.</param>
/// <param name="ProfitCenterId">The profit center.</param>
/// <param name="ProjectId">The project or job order.</param>
/// <param name="ProductLineId">The product line.</param>
/// <param name="CostElementType">The cost element type, when it differs from the line.</param>
/// <param name="Percentage">The percentage of the line, when the split is by percentage.</param>
/// <param name="Amount">The exact amount, when the split is by amount.</param>
public sealed record AllocationVector(
    string? CostCenterId = null,
    string? ProfitCenterId = null,
    string? ProjectId = null,
    string? ProductLineId = null,
    string? CostElementType = null,
    decimal? Percentage = null,
    decimal? Amount = null);
