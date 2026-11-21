// <copyright file="MrpPlan.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// An MRP planned order suggestion.
/// </summary>
public class MrpPlan : ITenantScopedEntity
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
    /// Gets or sets the planned order number.
    /// </summary>
    public string PlannedOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MRP run.
    /// </summary>
    public Guid MrpRunId { get; set; }

    /// <summary>
    /// Gets or sets the item identifier.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the item code.
    /// </summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the suggested quantity.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the suggested start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the due date.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Gets or sets the server status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BOM.
    /// </summary>
    public Guid? BomId { get; set; }

    /// <summary>
    /// Gets or sets the routing.
    /// </summary>
    public Guid? RoutingId { get; set; }

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
