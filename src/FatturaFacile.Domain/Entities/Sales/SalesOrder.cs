// <copyright file="SalesOrder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents a confirmed sales order aggregate placed by a customer, tracking fulfillment state,
/// order total, currency, and audit lifecycle metadata.
/// </summary>
public sealed class SalesOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private SalesOrder()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique business order reference number (e.g. "ORD-2026/012").
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the customer placing the sales order.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the date and time when the customer placed or confirmed the sales order.
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// Gets the transaction currency for the order items and financial settlement.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the total net or gross financial amount of the sales order.
    /// </summary>
    public decimal TotalAmount { get; }

    /// <summary>
    /// Gets the current lifecycle processing status of the sales order (defaults to "Confirmed").
    /// </summary>
    public string Status { get; } = "Confirmed";

    /// <summary>
    /// Gets or sets the UTC timestamp when the sales order record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that created the order.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the sales order was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that last modified the order.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new <see cref="SalesOrder"/> aggregate initialized with confirmed status and current UTC timestamps.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="orderNumber">The unique reference code for the order.</param>
    /// <param name="clientId">The client or customer identifier.</param>
    /// <param name="currency">The currency code for pricing and payment.</param>
    /// <returns>A newly created <see cref="SalesOrder"/> aggregate instance.</returns>
    public static SalesOrder Create(
        Guid tenantId,
        string orderNumber,
        Guid clientId,
        Currency currency)
    {
        return new SalesOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber,
            ClientId = clientId,
            OrderDate = DateTime.UtcNow,
            Currency = currency,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
