// <copyright file="ProductionOperation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// An operation of a production order.
/// </summary>
public class ProductionOperation : ITenantScopedEntity
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
    /// Gets or sets the owning order.
    /// </summary>
    public Guid ProductionOrderId { get; set; }

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
    /// Gets or sets the planned setup minutes.
    /// </summary>
    public decimal PlannedSetupMinutes { get; set; }

    /// <summary>
    /// Gets or sets the planned run minutes for the whole order quantity.
    /// </summary>
    public decimal PlannedRunMinutes { get; set; }

    /// <summary>
    /// Gets or sets the planned labour minutes for the whole order quantity.
    /// </summary>
    public decimal PlannedLaborMinutes { get; set; }

    /// <summary>
    /// Gets or sets the good quantity confirmed so far.
    /// </summary>
    public decimal ConfirmedGoodQuantity { get; set; }

    /// <summary>
    /// Gets or sets the scrapped quantity confirmed so far.
    /// </summary>
    public decimal ConfirmedScrapQuantity { get; set; }

    /// <summary>
    /// Gets or sets the setup plus run minutes confirmed so far.
    /// </summary>
    public decimal ActualMinutes { get; set; }

    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last local modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
