// <copyright file="LocalSupplierRebateSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// One payload of the supplier rebates and blanket agreements (Mod 049) pulled from the web-api, kept so the rebate cockpit and the
/// blanket agreement balance can be read offline. The payload is the `data` object or array of the API answer, stored as received.
/// A row is identified by its kind (agreement list, agreement, progress, runs, proposals, blanket agreement list or balance) and the
/// key of the record it belongs to.
/// </summary>
public class LocalSupplierRebateSnapshot : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the snapshot identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the kind of payload.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key of the record the payload belongs to (an identifier of the server, or "all" for a list).
    /// </summary>
    public string RecordKey { get; set; } = string.Empty;

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
