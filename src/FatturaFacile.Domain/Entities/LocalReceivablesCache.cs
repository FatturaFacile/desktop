// <copyright file="LocalReceivablesCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The last server answer of a receivables read (open items, aging, ledger, credit status, provision, statement ...), kept per
/// tenant and request route so the views keep working offline.
/// </summary>
public class LocalReceivablesCache : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the row identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <inheritdoc/>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the cache key, the route of the request including its query.
    /// </summary>
    public string CacheKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cached JSON, the whole body of the answer.
    /// </summary>
    public string Json { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the instant the answer was pulled.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }

    /// <inheritdoc/>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }
}
