// <copyright file="ShopFloorLog.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// An operation confirmation posted on the shop floor; its posting UUID is the idempotency key of the server posting.
/// </summary>
public class ShopFloorLog : ITenantScopedEntity
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
    /// Gets or sets the local order identifier.
    /// </summary>
    public Guid ProductionOrderId { get; set; }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public Guid OperationId { get; set; }

    /// <summary>
    /// Gets or sets the idempotency key (generated once, reused on every retry).
    /// </summary>
    public Guid OperationPostingUuid { get; set; }

    /// <summary>
    /// Gets or sets the setup minutes.
    /// </summary>
    public decimal SetupMinutes { get; set; }

    /// <summary>
    /// Gets or sets the run minutes.
    /// </summary>
    public decimal RunMinutes { get; set; }

    /// <summary>
    /// Gets or sets the labour minutes.
    /// </summary>
    public decimal LaborMinutes { get; set; }

    /// <summary>
    /// Gets or sets the downtime minutes.
    /// </summary>
    public decimal DowntimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the good quantity.
    /// </summary>
    public decimal GoodQuantity { get; set; }

    /// <summary>
    /// Gets or sets the scrap quantity.
    /// </summary>
    public decimal ScrapQuantity { get; set; }

    /// <summary>
    /// Gets or sets the scrap reason.
    /// </summary>
    public string? ScrapReason { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the final confirmation of the operation.
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    /// Gets or sets the posting date.
    /// </summary>
    public DateTime PostingDate { get; set; }

    /// <summary>
    /// Gets or sets the delivery state.
    /// </summary>
    public ManufacturingOutboxStatus SyncStatus { get; set; }

    /// <summary>
    /// Gets or sets the confirmation number assigned by the server.
    /// </summary>
    public string? ServerConfirmationNumber { get; set; }

    /// <summary>
    /// Gets or sets the creation instant (UTC).
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last local modification instant (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
