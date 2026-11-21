// <copyright file="AiCopilotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.AiCopilot;

/// <summary>
/// EF Core implementation of <see cref="IAiCopilotStore"/> for the active tenant.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class AiCopilotStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IAiCopilotStore
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AiSuggestion>> ListOpenAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Set<AiSuggestion>()
            .AsNoTracking()
            .Where(x => x.Status == AiSuggestionStatus.Pending || x.Status == AiSuggestionStatus.Failed)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var ordered = rows
            .OrderByDescending(x => PriorityRank(x.Priority))
            .ThenByDescending(x => x.Confidence)
            .ThenBy(x => x.ServerCreatedAt);
        return [.. ordered];
    }

    /// <inheritdoc/>
    public Task<AiSuggestion?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        db.Set<AiSuggestion>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AiSuggestion>> GetQueuedAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Set<AiSuggestion>()
            .Where(x => x.QueuedDecision != AiQueuedDecision.None)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return [.. rows.OrderBy(x => x.QueuedAt)];
    }

    /// <inheritdoc/>
    public Task<int> CountQueuedAsync(CancellationToken cancellationToken) =>
        db.Set<AiSuggestion>().CountAsync(x => x.QueuedDecision != AiQueuedDecision.None, cancellationToken);

    /// <inheritdoc/>
    public Task UpsertAsync(IReadOnlyList<AiSuggestionDto> items, DateTime nowUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(items);
        return UpsertCoreAsync(items, nowUtc, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveMissingAsync(IReadOnlySet<Guid> serverIds, CancellationToken cancellationToken)
    {
        var keep = serverIds.ToList();
        var stale = await db.Set<AiSuggestion>()
            .Where(x => x.QueuedDecision == AiQueuedDecision.None && !keep.Contains(x.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (stale.Count > 0)
        {
            db.Set<AiSuggestion>().RemoveRange(stale);
            _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.Set<AiSuggestion>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (row is not null)
        {
            _ = db.Set<AiSuggestion>().Remove(row);
            _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public Task SaveAsync(AiSuggestion suggestion, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return SaveCoreAsync(suggestion, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<AiInsightCache?> GetInsightsAsync(CancellationToken cancellationToken) =>
        db.Set<AiInsightCache>().AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task SaveInsightsAsync(string payloadJson, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var row = await db.Set<AiInsightCache>().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new AiInsightCache { TenantId = tenant.TenantId, CreatedAt = nowUtc };
            _ = db.Set<AiInsightCache>().Add(row);
        }

        row.PayloadJson = payloadJson;
        row.FetchedAt = nowUtc;
        row.UpdatedAt = nowUtc;
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static int PriorityRank(string priority) => priority switch
    {
        "high" => 2,
        "normal" => 1,
        _ => 0,
    };

    private async Task SaveCoreAsync(AiSuggestion suggestion, CancellationToken cancellationToken)
    {
        suggestion.UpdatedAt = DateTime.UtcNow;
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task UpsertCoreAsync(IReadOnlyList<AiSuggestionDto> items, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var ids = items.Select(i => i.Id).ToList();
        var existing = await db.Set<AiSuggestion>()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken)
            .ConfigureAwait(false);

        foreach (var item in items)
        {
            if (!existing.TryGetValue(item.Id, out var row))
            {
                row = new AiSuggestion { Id = item.Id, TenantId = tenant.TenantId, CreatedAt = nowUtc };
                _ = db.Set<AiSuggestion>().Add(row);
            }

            // The queued decision of an existing row is deliberately left untouched.
            row.Kind = item.Kind;
            row.Workflow = item.Workflow;
            row.SourceType = item.SourceType;
            row.SourceReference = item.SourceReference;
            row.SourceText = item.SourceText;
            row.ProposedPayloadJson = item.ProposedPayloadJson;
            row.FieldConfidencesJson = item.FieldConfidencesJson;
            row.Confidence = item.Confidence;
            row.Amount = item.Amount;
            row.Reasoning = item.Reasoning;
            row.Status = item.Status;
            row.Priority = item.Priority;
            row.ProviderUsed = item.ProviderUsed;
            row.ModelUsed = item.ModelUsed;
            row.ExpiresAt = item.ExpiresAt;
            row.ServerCreatedAt = item.CreatedAt;
            row.CachedAt = nowUtc;
            row.UpdatedAt = nowUtc;
        }

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
