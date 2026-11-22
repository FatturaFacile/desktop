// <copyright file="LocalHelpdeskCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The last server answer of a helpdesk read (breach-risk list, SLA metrics, customer context, escalation history, KB search),
/// kept per tenant and cache key so the workbench keeps working offline.
/// </summary>
public class LocalHelpdeskCache : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the row identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <inheritdoc/>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the cache key, for example breach-risk or context:{ticketId}.
    /// </summary>
    public string CacheKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cached JSON (the data member of the answer).
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
