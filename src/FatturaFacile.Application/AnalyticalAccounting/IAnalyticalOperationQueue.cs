// <copyright file="IAnalyticalOperationQueue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// The offline queue of manual analytical entries waiting for the web-api, for the active tenant.
/// </summary>
public interface IAnalyticalOperationQueue
{
    /// <summary>
    /// Adds an entry to the queue.
    /// </summary>
    /// <param name="payloadJson">The JSON body of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The identifier of the queued operation.</returns>
    public Task<Guid> EnqueueAsync(string payloadJson, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the operations still to be sent, oldest first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pending operations.</returns>
    public Task<IReadOnlyList<PendingAnalyticalOperation>> PendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Counts the operations still to be sent.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of pending operations.</returns>
    public Task<int> CountPendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Marks an operation as accepted by the server.
    /// </summary>
    /// <param name="id">The operation identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the update.</returns>
    public Task MarkSentAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Marks an operation as refused by the server so it leaves the pending list but stays for review.
    /// </summary>
    /// <param name="id">The operation identifier.</param>
    /// <param name="reason">The reason of the refusal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the update.</returns>
    public Task MarkFailedAsync(Guid id, string? reason, CancellationToken cancellationToken);
}
