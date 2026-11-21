// <copyright file="RoutingOperation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// A routing operation (the routing header is flattened into the operation).
/// </summary>
public class RoutingOperation : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the local identifier (equals the server identifier for pulled records).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the local tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the routing identifier.
    /// </summary>
    public Guid RoutingId { get; set; }

    /// <summary>
    /// Gets or sets the routing number.
    /// </summary>
    public string RoutingNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the produced item identifier.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the operation sequence.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Gets or sets the operation code.
    /// </summary>
    public string OperationCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the work centre.
    /// </summary>
    public Guid WorkCenterId { get; set; }

    /// <summary>
    /// Gets or sets the setup time in minutes (per order).
    /// </summary>
    public decimal SetupMinutes { get; set; }

    /// <summary>
    /// Gets or sets the machine run time per unit in minutes.
    /// </summary>
    public decimal RunMinutesPerUnit { get; set; }

    /// <summary>
    /// Gets or sets the labour time per unit in minutes.
    /// </summary>
    public decimal LaborMinutesPerUnit { get; set; }

    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last local modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the server updated_at stamp seen at the last pull.
    /// </summary>
    public string? ServerVersion { get; set; }
}
