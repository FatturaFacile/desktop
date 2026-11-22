// <copyright file="FieldServiceSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Default <see cref="IFieldServiceSyncService"/>: pulls a read model, checks that it parses and only then replaces the cached
/// one, so an unreadable answer never destroys a good cache.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="store">The local cache.</param>
/// <param name="time">The clock.</param>
public sealed class FieldServiceSyncService(IFieldServiceApiClient api, IFieldServiceStore store, TimeProvider time) : IFieldServiceSyncService
{
    private const int DefaultWindowDays = 7;

    /// <inheritdoc/>
    public Task<FieldServiceCallResult> SyncAsync(FieldServiceSyncScope scope, string? key, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
    {
        if (scope is FieldServiceSyncScope.WorkOrder or FieldServiceSyncScope.History or FieldServiceSyncScope.Bundle)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
        }

        return api.IsAuthenticated
            ? FieldServiceCallRunner.RunAsync(() => PullAsync(scope, key, from, to, cancellationToken), cancellationToken)
            : Task.FromResult(new FieldServiceCallResult(FieldServiceCallStatus.Unauthorized));
    }

    private static (string Kind, string Key) Slot(FieldServiceSyncScope scope, string? key) => scope switch
    {
        FieldServiceSyncScope.Board => (FieldServiceCacheKinds.Board, FieldServiceCacheKinds.All),
        FieldServiceSyncScope.Contracts => (FieldServiceCacheKinds.Contracts, FieldServiceCacheKinds.All),
        FieldServiceSyncScope.Equipment => (FieldServiceCacheKinds.Equipment, FieldServiceCacheKinds.All),
        FieldServiceSyncScope.WorkOrders => (FieldServiceCacheKinds.WorkOrders, FieldServiceCacheKinds.All),
        FieldServiceSyncScope.WorkOrder => (FieldServiceCacheKinds.WorkOrder, key ?? string.Empty),
        FieldServiceSyncScope.History => (FieldServiceCacheKinds.History, key ?? string.Empty),
        _ => (FieldServiceCacheKinds.Bundle, key ?? string.Empty),
    };

    private static DateTimeOffset StartOfWeek(DateTimeOffset now)
    {
        var daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;

        return new DateTimeOffset(now.UtcDateTime.Date.AddDays(-daysSinceMonday), TimeSpan.Zero);
    }

    private static void EnsureReadable(FieldServiceSyncScope scope, string json)
    {
        switch (scope)
        {
            case FieldServiceSyncScope.Board:
                _ = FieldServiceBoardParser.Parse(json);
                break;
            case FieldServiceSyncScope.Contracts:
                _ = FieldServiceContractParser.ParseContracts(json);
                break;
            case FieldServiceSyncScope.Equipment:
                _ = FieldServiceContractParser.ParseEquipment(json);
                break;
            case FieldServiceSyncScope.WorkOrders:
                _ = FieldServiceWorkOrderParser.ParseList(json);
                break;
            case FieldServiceSyncScope.WorkOrder:
                _ = FieldServiceWorkOrderParser.ParseDetail(json);
                break;
            case FieldServiceSyncScope.History:
                _ = FieldServiceContractParser.ParseHistory(json);
                break;
            default:
                _ = FieldServiceBundleParser.Parse(json);
                break;
        }
    }

    private async Task<FieldServiceCallResult> PullAsync(FieldServiceSyncScope scope, string? key, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
    {
        var json = await FetchAsync(scope, key, from, to, cancellationToken).ConfigureAwait(false);

        // A payload that does not parse must never replace a good cache.
        EnsureReadable(scope, json);

        var now = time.GetUtcNow().UtcDateTime;
        var (kind, cacheKey) = Slot(scope, key);
        await store.SaveAsync(kind, cacheKey, json, now, cancellationToken).ConfigureAwait(false);

        return new FieldServiceCallResult(FieldServiceCallStatus.Ok, AtUtc: now);
    }

    private Task<string> FetchAsync(FieldServiceSyncScope scope, string? key, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
    {
        var id = key ?? string.Empty;

        return scope switch
        {
            FieldServiceSyncScope.Board => BoardAsync(from, to, cancellationToken),
            FieldServiceSyncScope.Contracts => api.GetContractsAsync(cancellationToken),
            FieldServiceSyncScope.Equipment => api.GetEquipmentAsync(cancellationToken),
            FieldServiceSyncScope.WorkOrders => api.GetWorkOrdersAsync(cancellationToken),
            FieldServiceSyncScope.WorkOrder => api.GetWorkOrderAsync(id, cancellationToken),
            FieldServiceSyncScope.History => api.GetEquipmentHistoryAsync(id, cancellationToken),
            _ => api.GetTechnicianBundleAsync(id, cancellationToken),
        };
    }

    private Task<string> BoardAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
    {
        var start = from ?? StartOfWeek(time.GetUtcNow());
        var end = to ?? start.AddDays(DefaultWindowDays);

        return api.GetBoardAsync(start, end, cancellationToken);
    }
}
