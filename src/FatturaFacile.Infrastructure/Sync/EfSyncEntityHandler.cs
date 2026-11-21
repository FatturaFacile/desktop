// <copyright file="EfSyncEntityHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Nodes;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// Generic EF Core handler for entities whose columns mirror the web-api JSON (snake_case).
/// </summary>
/// <typeparam name="TEntity">The local entity type.</typeparam>
/// <param name="entityName">The delta feed entity name.</param>
/// <param name="writeRoute">The REST route for writes, or null when the entity is pulled only.</param>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class EfSyncEntityHandler<TEntity>(
    string entityName,
    string? writeRoute,
    FatturaFacileDbContext db,
    ICurrentTenantService tenant,
    TimeProvider timeProvider) : ISyncEntityHandler
    where TEntity : class, ISyncableEntity
{
    /// <inheritdoc/>
    public string EntityName { get; } = entityName;

    /// <inheritdoc/>
    public string? WriteRoute { get; } = writeRoute;

    /// <inheritdoc/>
    public async Task<string?> BuildPayloadAsync(Guid entityId, CancellationToken cancellationToken)
    {
        var entity = await db.Set<TEntity>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == entityId, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null || entity.DeletedAt is not null)
        {
            return null;
        }

        var node = JsonSerializer.SerializeToNode(entity, SyncJson.Options) as JsonObject;

        // The server derives the tenant from the X-Tenant-Id header; the local tenant id is unrelated to it.
        _ = node?.Remove("tenant_id");
        return node?.ToJsonString();
    }

    /// <inheritdoc/>
    public Task<SyncApplyResult> ApplyAsync(
        IReadOnlyList<JsonElement> upserts,
        IReadOnlyList<Guid> deleted,
        IReadOnlySet<Guid> protectedIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(upserts);
        ArgumentNullException.ThrowIfNull(deleted);
        ArgumentNullException.ThrowIfNull(protectedIds);
        return ApplyCoreAsync(upserts, deleted, protectedIds, cancellationToken);
    }

    private static bool TryRead(JsonElement row, out TEntity entity, out string? version)
    {
        entity = null!;
        version = row.TryGetProperty("updated_at", out var updated) && updated.ValueKind == JsonValueKind.String ? updated.GetString() : null;

        try
        {
            var parsed = row.Deserialize<TEntity>(SyncJson.Options);
            if (parsed is null || parsed.Id == Guid.Empty)
            {
                return false;
            }

            entity = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    private async Task<SyncApplyResult> ApplyCoreAsync(
        IReadOnlyList<JsonElement> upserts,
        IReadOnlyList<Guid> deleted,
        IReadOnlySet<Guid> protectedIds,
        CancellationToken cancellationToken)
    {
        using var suppression = SyncSuppression.Begin();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var applied = 0;
        var skipped = 0;

        foreach (var row in upserts)
        {
            if (!TryRead(row, out var incoming, out var version) || protectedIds.Contains(incoming.Id))
            {
                skipped++;
                continue;
            }

            incoming.TenantId = tenant.TenantId;
            var existing = await FindAsync(incoming.Id, cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                _ = db.Set<TEntity>().Add(incoming);
            }
            else
            {
                db.Entry(existing).CurrentValues.SetValues(incoming);
            }

            await UpsertStateAsync(incoming.Id, version, now, cancellationToken).ConfigureAwait(false);
            applied++;
        }

        foreach (var id in deleted)
        {
            if (protectedIds.Contains(id))
            {
                skipped++;
                continue;
            }

            var existing = await FindAsync(id, cancellationToken).ConfigureAwait(false);
            if (existing is null || existing.DeletedAt is not null)
            {
                continue;
            }

            existing.DeletedAt = now;
            await UpsertStateAsync(id, version: null, now, cancellationToken).ConfigureAwait(false);
            applied++;
        }

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new SyncApplyResult(applied, skipped);
    }

    private Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken)
        => db.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    private async Task UpsertStateAsync(Guid id, string? version, DateTime now, CancellationToken cancellationToken)
    {
        var state = await db.Set<SyncEntityState>()
            .FirstOrDefaultAsync(s => s.EntityName == EntityName && s.EntityId == id, cancellationToken)
            .ConfigureAwait(false);
        SyncStateWriter.Apply(db, state, (tenant.TenantId, EntityName, id), SyncOutboxStatus.Synced, version, now);
    }
}
