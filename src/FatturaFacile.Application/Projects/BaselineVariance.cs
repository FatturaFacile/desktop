// <copyright file="BaselineVariance.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The comparison of the current plan of a project with one of its baselines.
/// </summary>
/// <param name="BaselineId">The baseline identifier.</param>
/// <param name="BaselineNumber">The baseline number.</param>
/// <param name="BaselineName">The baseline name.</param>
/// <param name="CapturedAt">The instant (UTC) the baseline was captured.</param>
/// <param name="StartVarianceDays">The project start slip in days.</param>
/// <param name="FinishVarianceDays">The project finish slip in days.</param>
/// <param name="HoursVariance">The planned hours variance of the project.</param>
/// <param name="CostVariance">The planned cost variance of the project.</param>
/// <param name="RevenueVariance">The planned revenue variance of the project.</param>
/// <param name="Nodes">The variance of every node.</param>
public sealed record BaselineVariance(
    Guid BaselineId,
    int BaselineNumber,
    string BaselineName,
    DateTime CapturedAt,
    int? StartVarianceDays,
    int? FinishVarianceDays,
    decimal HoursVariance,
    decimal CostVariance,
    decimal RevenueVariance,
    IReadOnlyList<NodeVariance> Nodes);
