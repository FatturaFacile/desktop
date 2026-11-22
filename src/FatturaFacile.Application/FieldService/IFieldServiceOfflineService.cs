// <copyright file="IFieldServiceOfflineService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Offline-first writes of the technician: every operation is validated, saved in the local outbox and only then sent, so a
/// disconnected laptop or tablet loses nothing and a replay is applied once.
/// </summary>
public interface IFieldServiceOfflineService
{
    /// <summary>
    /// Queues the move of a work order to en route or on site.
    /// </summary>
    /// <param name="workOrderId">The work order.</param>
    /// <param name="status">The new status: en_route or on_site.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<FieldServiceQueueResult> QueueStatusAsync(Guid workOrderId, string status, CancellationToken cancellationToken);

    /// <summary>
    /// Queues the consumption of a part from the van.
    /// </summary>
    /// <param name="input">The part.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<FieldServiceQueueResult> QueuePartAsync(FieldServicePartInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Queues the signed service report, which closes the work order.
    /// </summary>
    /// <param name="input">The report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<FieldServiceQueueResult> QueueReportAsync(FieldServiceReportInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Sends the queued operations that are due, in batches, in the order the technician acted.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How many were applied, rejected and are still waiting.</returns>
    public Task<FieldServiceFlushResult> FlushAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Summarizes the outbox: the pending count and the rejected operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary.</returns>
    public Task<FieldServiceOutboxSummary> SummaryAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Dismisses a rejected operation.
    /// </summary>
    /// <param name="entryId">The outbox entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the change.</returns>
    public Task DismissConflictAsync(Guid entryId, CancellationToken cancellationToken);
}
