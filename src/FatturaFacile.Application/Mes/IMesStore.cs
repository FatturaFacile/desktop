// <copyright file="IMesStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Local persistence of the MES module for the active tenant: the cache of the last boards and the outbox of the commands the
/// operator issued.
/// </summary>
public interface IMesStore
{
    /// <summary>
    /// Finds the cached payload of a work centre (or of the work centre list, with <see cref="LocalMesBoard.WorkCentreList"/>).
    /// </summary>
    /// <param name="workCenterId">The work centre key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached row, or null.</returns>
    public Task<LocalMesBoard?> FindBoardAsync(Guid workCenterId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the payload of a work centre, replacing the cached one.
    /// </summary>
    /// <param name="workCenterId">The work centre key.</param>
    /// <param name="json">The payload as received.</param>
    /// <param name="syncedAtUtc">When it was pulled (UTC).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the payload is saved.</returns>
    public Task SaveBoardAsync(Guid workCenterId, string json, DateTime syncedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a command to the outbox.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the entry is saved.</returns>
    public Task EnqueueAsync(MesOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the commands waiting to be sent, oldest action first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pending entries.</returns>
    public Task<IReadOnlyList<MesOutboxEntry>> PendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the commands the server refused and the operator has not dismissed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conflicting entries.</returns>
    public Task<IReadOnlyList<MesOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the changes of entries returned by this store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the changes are saved.</returns>
    public Task SaveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gives the pending commands of an operation the phase a take-over created on the server.
    /// </summary>
    /// <param name="operationId">The operation.</param>
    /// <param name="phaseId">The phase.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the commands are updated.</returns>
    public Task AssignPhaseAsync(Guid operationId, Guid phaseId, CancellationToken cancellationToken);
}
