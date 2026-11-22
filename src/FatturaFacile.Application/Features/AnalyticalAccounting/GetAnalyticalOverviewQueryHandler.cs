// <copyright file="GetAnalyticalOverviewQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AnalyticalAccounting;
using MediatR;

namespace FatturaFacile.Application.Features.AnalyticalAccounting;

/// <summary>
/// Handles <see cref="GetAnalyticalOverviewQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
/// <param name="queue">The offline queue.</param>
public sealed class GetAnalyticalOverviewQueryHandler(IAnalyticalSnapshotStore store, IAnalyticalOperationQueue queue) : IRequestHandler<GetAnalyticalOverviewQuery, AnalyticalOverview>
{
    /// <inheritdoc/>
    public Task<AnalyticalOverview> Handle(GetAnalyticalOverviewQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(cancellationToken);
    }

    private async Task<AnalyticalOverview> LoadAsync(CancellationToken cancellationToken)
    {
        var centers = await store.FindAsync(AnalyticalContract.CentersKey, cancellationToken).ConfigureAwait(false);
        var runs = await store.FindAsync(AnalyticalContract.RunsKey, cancellationToken).ConfigureAwait(false);
        var margin = await store.FindAsync(AnalyticalContract.MarginKey, cancellationToken).ConfigureAwait(false);
        var comparison = await store.FindAsync(AnalyticalContract.ComparisonKey, cancellationToken).ConfigureAwait(false);
        var pending = await queue.CountPendingAsync(cancellationToken).ConfigureAwait(false);

        return new AnalyticalOverview(
            centers is null ? [] : AnalyticalParser.ParseCenters(centers.Json),
            runs is null ? [] : AnalyticalParser.ParseRuns(runs.Json),
            margin is null ? null : AnalyticalParser.ParseMargin(margin.Json),
            comparison is null ? [] : AnalyticalParser.ParseComparison(comparison.Json),
            margin?.SyncedAtUtc ?? centers?.SyncedAtUtc,
            pending);
    }
}
