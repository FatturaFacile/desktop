// <copyright file="AnalyticalOverview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// Everything the analytical cost accounting view shows, read from the local cache.
/// </summary>
/// <param name="Centers">The center tree.</param>
/// <param name="Runs">The apportionment history.</param>
/// <param name="Margin">The contribution margin statement, or null when never synced.</param>
/// <param name="Comparison">The direct-versus-apportioned comparison.</param>
/// <param name="SyncedAtUtc">When the cache was last pulled, or null when never.</param>
/// <param name="PendingOperations">How many entries wait in the offline queue.</param>
public sealed record AnalyticalOverview(
    IReadOnlyList<AnalyticalCenterNode> Centers,
    IReadOnlyList<ApportionmentRunSummary> Runs,
    ContributionMarginStatement? Margin,
    IReadOnlyList<DirectVsApportionedRow> Comparison,
    DateTime? SyncedAtUtc,
    int PendingOperations);
