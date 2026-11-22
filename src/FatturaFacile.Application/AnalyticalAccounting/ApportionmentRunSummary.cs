// <copyright file="ApportionmentRunSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// One step-down apportionment run of the history.
/// </summary>
/// <param name="Id">The run identifier.</param>
/// <param name="PeriodKey">The period, YYYY-MM.</param>
/// <param name="Status">PREVIEW, CONSOLIDATED or REVERSED.</param>
/// <param name="TotalPool">The cost to apportion.</param>
/// <param name="TotalAllocated">The cost apportioned.</param>
/// <param name="TotalUnallocated">The cost that stayed on its sender.</param>
public sealed record ApportionmentRunSummary(
    string Id,
    string PeriodKey,
    string Status,
    decimal TotalPool,
    decimal TotalAllocated,
    decimal TotalUnallocated);
