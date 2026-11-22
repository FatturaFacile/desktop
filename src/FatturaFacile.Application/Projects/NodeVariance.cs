// <copyright file="NodeVariance.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The variance of one WBS node against a baseline (current minus baseline; positive days mean it moved later).
/// </summary>
/// <param name="NodeId">The node identifier.</param>
/// <param name="WbsCode">The current WBS code (the baseline code for a removed node).</param>
/// <param name="Title">The current title (the baseline title for a removed node).</param>
/// <param name="Status">How the node compares with the baseline.</param>
/// <param name="StartVarianceDays">The start slip in days, when both dates are known.</param>
/// <param name="FinishVarianceDays">The finish slip in days, when both dates are known.</param>
/// <param name="HoursVariance">The planned hours variance.</param>
/// <param name="CostVariance">The planned cost variance.</param>
/// <param name="RevenueVariance">The planned revenue variance.</param>
public sealed record NodeVariance(
    Guid NodeId,
    string WbsCode,
    string Title,
    VarianceStatus Status,
    int? StartVarianceDays,
    int? FinishVarianceDays,
    decimal HoursVariance,
    decimal CostVariance,
    decimal RevenueVariance);
