// <copyright file="ISyncEntityHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Maps one server entity to its local table. One handler per synchronized entity.
/// </summary>
public interface ISyncEntityHandler
{
    /// <summary>
    /// Gets the entity name used by the delta feed (for example "contacts").
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the REST collection route for writes, or null when the entity is pulled only.
    /// </summary>
    public string? WriteRoute { get; }

    /// <summary>
    /// Builds the JSON body of the current local state of a record.
    /// </summary>
    /// <param name="entityId">The record identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The JSON, or null when the record no longer exists locally.</returns>
    public Task<string?> BuildPayloadAsync(Guid entityId, CancellationToken cancellationToken);

    /// <summary>
    /// Applies pulled data locally without producing outbox entries.
    /// </summary>
    /// <param name="upserts">Created or changed server records.</param>
    /// <param name="deleted">Identifiers deleted on the server.</param>
    /// <param name="protectedIds">Records with pending local changes that must not be overwritten.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The counts of applied and skipped records.</returns>
    public Task<SyncApplyResult> ApplyAsync(
        IReadOnlyList<JsonElement> upserts,
        IReadOnlyList<Guid> deleted,
        IReadOnlySet<Guid> protectedIds,
        CancellationToken cancellationToken);
}
