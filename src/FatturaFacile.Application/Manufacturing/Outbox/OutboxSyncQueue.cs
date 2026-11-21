// <copyright file="OutboxSyncQueue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json.Nodes;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// Queues manufacturing mutations while offline and replays them idempotently when the server is reachable.
/// </summary>
/// <remarks>
/// Order is FIFO per production order. The payload and the idempotency key (the operation_posting_uuid of a confirmation) are stored in the outbox row when the
/// mutation is queued and reused on every retry, so a retry after a lost response is a server-side replay (HTTP 200) and never posts twice.
/// A conflict, rejection or exhausted retry blocks only the later entries of the same order.
/// </remarks>
/// <param name="store">The outbox persistence.</param>
/// <param name="reads">The offline read store.</param>
/// <param name="gateway">The web-api gateway.</param>
/// <param name="tenant">The current tenant.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class OutboxSyncQueue(
    IManufacturingOutboxStore store,
    IManufacturingReadStore reads,
    IManufacturingApiGateway gateway,
    ICurrentTenantService tenant,
    TimeProvider timeProvider)
{
    private const int HttpOk = 200;
    private int running;

    /// <summary>
    /// Queues the creation of a production order that was just inserted locally.
    /// </summary>
    /// <param name="order">The new local order with its operations (Id, tenant and local number already set).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The queued entry.</returns>
    public Task<ManufacturingOutboxEntry> QueueCreateOrderAsync(ProductionOrder order, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        return QueueCreateOrderCoreAsync(order, cancellationToken);
    }

    /// <summary>
    /// Queues a lifecycle transition and applies it to the local order at once.
    /// </summary>
    /// <param name="orderId">The local order identifier.</param>
    /// <param name="kind">Release, Start, Complete or Close.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The queued entry.</returns>
    /// <exception cref="InvalidOperationException">The order is unknown or not in the status the transition requires.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The kind is not a lifecycle transition.</exception>
    public async Task<ManufacturingOutboxEntry> QueueTransitionAsync(Guid orderId, ManufacturingMutationKind kind, CancellationToken cancellationToken)
    {
        var (verb, required, target) = kind switch
        {
            ManufacturingMutationKind.Release => ("release", ProductionOrderStatus.Planned, ProductionOrderStatus.Released),
            ManufacturingMutationKind.Start => ("start", ProductionOrderStatus.Released, ProductionOrderStatus.InProgress),
            ManufacturingMutationKind.Complete => ("complete", ProductionOrderStatus.InProgress, ProductionOrderStatus.Completed),
            ManufacturingMutationKind.Close => ("close", ProductionOrderStatus.Completed, ProductionOrderStatus.Closed),
            ManufacturingMutationKind.CreateOrder or ManufacturingMutationKind.ConfirmOperation => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Only lifecycle transitions can be queued here."),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown mutation kind."),
        };

        var order = await reads.GetOrderAsync(orderId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Production order {orderId} is not in the local store.");
        if (order.Status != required)
        {
            throw new InvalidOperationException($"Order {order.OrderNumber} is {order.Status}; {verb} requires {required}.");
        }

        var entry = NewEntry(kind, orderId, $"production-orders/{{order}}/{verb}", payloadJson: null, order.ServerVersion);
        await store.QueueAsync(entry, log: null, new OptimisticOrderChange(orderId, target), cancellationToken).ConfigureAwait(false);
        return entry;
    }

    /// <summary>
    /// Queues an operation confirmation (shop-floor posting) and applies it to the local operation at once.
    /// </summary>
    /// <param name="input">The posting.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The posting UUID, which is also the idempotency key of every send attempt.</returns>
    /// <exception cref="InvalidOperationException">The order or operation is unknown, not executable, or not yet known to the server.</exception>
    public Task<Guid> QueueConfirmationAsync(OperationConfirmationInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        return QueueConfirmationCoreAsync(input, cancellationToken);
    }

    /// <summary>
    /// Sends the due pending entries in order. Safe to call concurrently: a second call while a flush runs returns immediately.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The totals of the run.</returns>
    public async Task<OutboxFlushResult> FlushAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
        {
            return new OutboxFlushResult(0, 0, 0, 0, 0, 0, AuthRequired: false, []);
        }

        try
        {
            return await FlushCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _ = Interlocked.Exchange(ref running, 0);
        }
    }

    private async Task<ManufacturingOutboxEntry> QueueCreateOrderCoreAsync(ProductionOrder order, CancellationToken cancellationToken)
    {
        JsonObject body = new()
        {
            ["mfg_item_id"] = order.ItemId,
            ["target_quantity"] = order.TargetQuantity,
            ["planned_start_date"] = order.PlannedStartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["planned_end_date"] = order.PlannedEndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["bom_id"] = order.BomId,
            ["routing_id"] = order.RoutingId,
        };

        var entry = NewEntry(ManufacturingMutationKind.CreateOrder, order.Id, "production-orders", body.ToJsonString(), baseServerVersion: null);
        await store.QueueNewOrderAsync(order, entry, cancellationToken).ConfigureAwait(false);
        return entry;
    }

    private async Task<Guid> QueueConfirmationCoreAsync(OperationConfirmationInput input, CancellationToken cancellationToken)
    {
        var order = await reads.GetOrderAsync(input.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Production order {input.OrderId} is not in the local store.");
        if (order.Status is not (ProductionOrderStatus.Released or ProductionOrderStatus.InProgress))
        {
            throw new InvalidOperationException($"Order {order.OrderNumber} is {order.Status}; postings need a released or running order.");
        }

        if (order.ServerId is null)
        {
            throw new InvalidOperationException($"Order {order.OrderNumber} is not synchronised yet: its operations are only known after the server created it.");
        }

        var operation = order.Operations.FirstOrDefault(o => o.Id == input.OperationId)
            ?? throw new InvalidOperationException($"Operation {input.OperationId} does not belong to order {order.OrderNumber}.");

        var postingUuid = Guid.NewGuid();
        var postingDate = input.PostingDate.Date;
        JsonObject body = new()
        {
            ["setup_minutes"] = input.SetupMinutes,
            ["run_minutes"] = input.RunMinutes,
            ["labor_minutes"] = input.LaborMinutes,
            ["downtime_minutes"] = input.DowntimeMinutes,
            ["good_quantity"] = input.GoodQuantity,
            ["scrap_quantity"] = input.ScrapQuantity,
            ["scrap_reason"] = string.IsNullOrWhiteSpace(input.ScrapReason) ? null : input.ScrapReason,
            ["is_final"] = input.IsFinal,
            ["posting_date"] = postingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["operation_posting_uuid"] = postingUuid,
        };

        var entry = NewEntry(
            ManufacturingMutationKind.ConfirmOperation,
            order.Id,
            $"production-orders/{{order}}/operations/{operation.Id}/confirmations",
            body.ToJsonString(),
            order.ServerVersion,
            postingUuid);
        entry.OperationId = operation.Id;

        ShopFloorLog log = new()
        {
            TenantId = tenant.TenantId,
            ProductionOrderId = order.Id,
            OperationId = operation.Id,
            OperationPostingUuid = postingUuid,
            SetupMinutes = input.SetupMinutes,
            RunMinutes = input.RunMinutes,
            LaborMinutes = input.LaborMinutes,
            DowntimeMinutes = input.DowntimeMinutes,
            GoodQuantity = input.GoodQuantity,
            ScrapQuantity = input.ScrapQuantity,
            ScrapReason = input.ScrapReason,
            IsFinal = input.IsFinal,
            PostingDate = postingDate,
            SyncStatus = ManufacturingOutboxStatus.Pending,
        };

        var isLastOperation = operation.Sequence >= order.Operations.Max(o => o.Sequence);
        OptimisticOrderChange change = new(
            order.Id,
            NewStatus: null,
            operation.Id,
            input.GoodQuantity,
            input.ScrapQuantity,
            input.SetupMinutes + input.RunMinutes,
            CountsAsOutput: isLastOperation);

        await store.QueueAsync(entry, log, change, cancellationToken).ConfigureAwait(false);
        return postingUuid;
    }

    private async Task<OutboxFlushResult> FlushCoreAsync(CancellationToken cancellationToken)
    {
        FlushState state = new();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var pending = await store.GetPendingAsync(cancellationToken).ConfigureAwait(false);
        var held = new HashSet<Guid>(await store.GetBlockedOrderIdsAsync(cancellationToken).ConfigureAwait(false));

        foreach (var entry in pending)
        {
            if (held.Contains(entry.ProductionOrderId))
            {
                state.Deferred++;
                continue;
            }

            if (entry.NextAttemptAt > now)
            {
                // Keep the order of the mutations of this order: later entries wait for the backoff.
                _ = held.Add(entry.ProductionOrderId);
                state.Deferred++;
                continue;
            }

            var keepGoing = await ProcessAsync(entry, state, cancellationToken).ConfigureAwait(false);
            if (entry.Status is ManufacturingOutboxStatus.Conflict or ManufacturingOutboxStatus.Failed)
            {
                _ = held.Add(entry.ProductionOrderId);
            }

            if (!keepGoing)
            {
                break;
            }
        }

        return new OutboxFlushResult(state.Sent, state.Replayed, state.Conflicts, state.Discarded, state.Failed, state.Deferred, state.AuthRequired, state.Errors);
    }

    private async Task<bool> ProcessAsync(ManufacturingOutboxEntry entry, FlushState state, CancellationToken cancellationToken)
    {
        var route = await store.ResolveRouteAsync(entry, cancellationToken).ConfigureAwait(false);
        if (route is null)
        {
            state.Deferred++;
            return true;
        }

        var outcome = await gateway.SendAsync(new ManufacturingRequest(route.Route, entry.PayloadJson, entry.IdempotencyKey), cancellationToken).ConfigureAwait(false);
        entry.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;

        switch (outcome.Result)
        {
            case SyncSendResult.Success:
                await MarkSyncedAsync(entry, outcome, resolution: null, state, cancellationToken).ConfigureAwait(false);
                return true;

            case SyncSendResult.Conflict:
                return await ResolveConflictAsync(entry, route, outcome, state, cancellationToken).ConfigureAwait(false);

            case SyncSendResult.Rejected:
                entry.Status = ManufacturingOutboxStatus.Failed;
                entry.LastError = outcome.Error;
                state.Failed++;
                await store.SaveAsync(entry, cancellationToken).ConfigureAwait(false);
                return true;

            case SyncSendResult.Unauthorized:
                state.AuthRequired = true;
                return false;

            case SyncSendResult.Transient:
                await HandleTransientAsync(entry, outcome.Error, state, cancellationToken).ConfigureAwait(false);
                return false;

            default:
                throw new InvalidOperationException($"Unexpected send result {outcome.Result}.");
        }
    }

    private async Task MarkSyncedAsync(ManufacturingOutboxEntry entry, ManufacturingSendOutcome outcome, string? resolution, FlushState state, CancellationToken cancellationToken)
    {
        entry.Status = ManufacturingOutboxStatus.Synced;
        entry.LastError = null;
        entry.Resolution = resolution;

        if (entry.Kind == ManufacturingMutationKind.ConfirmOperation && outcome.StatusCode == HttpOk)
        {
            // HTTP 200 on a confirmation: the server already applied this operation_posting_uuid and returned the original posting.
            entry.Resolution = "ReplayedByServer";
            state.Replayed++;
        }

        state.Sent++;
        await store.ApplyOutcomeAsync(entry, outcome.ResponseBody, cancellationToken).ConfigureAwait(false);
        await store.SaveAsync(entry, cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> ResolveConflictAsync(ManufacturingOutboxEntry entry, ResolvedRoute route, ManufacturingSendOutcome outcome, FlushState state, CancellationToken cancellationToken)
    {
        string? serverJson = null;
        if (route.ServerOrderId is { } serverId)
        {
            try
            {
                serverJson = await gateway.GetJsonAsync($"production-orders/{serverId:D}", cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                await HandleTransientAsync(entry, ex.Message, state, cancellationToken).ConfigureAwait(false);
                return false;
            }
        }

        var resolution = ManufacturingConflictResolver.Resolve(entry.Kind, WorkbenchJson.ReadOrderStatus(serverJson), entry.ConflictRetries);
        entry.LastError = resolution.Reason + " " + outcome.Error;

        switch (resolution.Decision)
        {
            case ConflictDecision.AlreadyApplied:
                await MarkSyncedAsync(entry, outcome, nameof(ConflictDecision.AlreadyApplied), state, cancellationToken).ConfigureAwait(false);
                break;

            case ConflictDecision.Reapply:
                entry.ConflictRetries++;
                entry.Status = ManufacturingOutboxStatus.Pending;
                entry.Resolution = nameof(ConflictDecision.Reapply);
                entry.NextAttemptAt = timeProvider.GetUtcNow().UtcDateTime;
                state.Deferred++;
                await store.SaveAsync(entry, cancellationToken).ConfigureAwait(false);
                break;

            case ConflictDecision.ServerWins:
                entry.Status = ManufacturingOutboxStatus.Discarded;
                entry.Resolution = nameof(ConflictDecision.ServerWins);
                state.Discarded++;
                await store.ApplyOutcomeAsync(entry, responseBody: null, cancellationToken).ConfigureAwait(false);
                await store.SaveAsync(entry, cancellationToken).ConfigureAwait(false);
                break;

            case ConflictDecision.ManualReview:
                entry.Status = ManufacturingOutboxStatus.Conflict;
                entry.Resolution = nameof(ConflictDecision.ManualReview);
                state.Conflicts++;
                await store.SaveAsync(entry, cancellationToken).ConfigureAwait(false);
                break;

            default:
                throw new InvalidOperationException($"Unexpected decision {resolution.Decision}.");
        }

        return true;
    }

    private Task HandleTransientAsync(ManufacturingOutboxEntry entry, string? error, FlushState state, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        entry.Attempts++;
        entry.LastError = error;
        entry.UpdatedAt = now;
        if (SyncRetryPolicy.CanRetry(entry.Attempts))
        {
            entry.NextAttemptAt = now + SyncRetryPolicy.NextDelay(entry.Attempts);
        }
        else
        {
            entry.Status = ManufacturingOutboxStatus.Failed;
            state.Failed++;
        }

        state.Errors.Add($"{entry.Kind}/{entry.ProductionOrderId}: {error}");
        return store.SaveAsync(entry, cancellationToken);
    }

    private ManufacturingOutboxEntry NewEntry(ManufacturingMutationKind kind, Guid orderId, string route, string? payloadJson, string? baseServerVersion, Guid? idempotencyKey = null) => new()
    {
        TenantId = tenant.TenantId,
        Kind = kind,
        ProductionOrderId = orderId,
        RouteTemplate = route,
        PayloadJson = payloadJson,
        IdempotencyKey = idempotencyKey ?? Guid.NewGuid(),
        Status = ManufacturingOutboxStatus.Pending,
        NextAttemptAt = timeProvider.GetUtcNow().UtcDateTime,
        BaseServerVersion = baseServerVersion,
        CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
    };

    private sealed class FlushState
    {
        public int Sent { get; set; }

        public int Replayed { get; set; }

        public int Conflicts { get; set; }

        public int Discarded { get; set; }

        public int Failed { get; set; }

        public int Deferred { get; set; }

        public bool AuthRequired { get; set; }

        public List<string> Errors { get; } = [];
    }
}
