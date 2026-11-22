// <copyright file="LocalFieldServiceCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// One cached read model of the Field Service Management module (GH-196/197/202) pulled from the web-api: the dispatcher board,
/// the maintenance contracts, the equipment, the work orders and their detail, the service history of an asset or the offline
/// bundle of a technician. The payload is the `data` object of the answer, stored as received, so the module works offline.
/// </summary>
public class LocalFieldServiceCache : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the row identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the kind of read model (board, contracts, equipment, work_orders, work_order, history, bundle).
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key inside the kind (a server identifier, or "all" for the singletons).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payload as received.
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the instant (UTC) the payload was pulled.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
