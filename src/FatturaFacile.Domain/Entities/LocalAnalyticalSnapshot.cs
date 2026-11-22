// <copyright file="LocalAnalyticalSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// One cached read model of the industrial and analytical cost accounting module (Mod 018) pulled from the web-api: the center
/// tree, the apportionment runs, the contribution margin statement or the direct-versus-apportioned comparison. The payload is
/// the `data` object of the answer as received, so the figures can be read offline.
/// </summary>
public class LocalAnalyticalSnapshot : ITenantScopedEntity
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
    /// Gets or sets the key of the read model (centers, runs, margin, comparison).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payload as received.
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the instant (UTC) the snapshot was pulled.
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
