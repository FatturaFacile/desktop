// <copyright file="LocalProcurementCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The last server answer of a procurement fulfilment read (purchase orders, overdue orders, confirmations, dropshipments ...), kept per
/// tenant and request route so the views keep working offline (Mod 046).
/// </summary>
public class LocalProcurementCache : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the row identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <inheritdoc/>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the request route with its query.
    /// </summary>
    public string CacheKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body of the answer as received.
    /// </summary>
    public string Json { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the instant (UTC) the answer was pulled.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }

    /// <inheritdoc/>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }
}
