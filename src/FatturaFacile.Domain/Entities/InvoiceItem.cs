// <copyright file="InvoiceItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents an individual line item on a commercial invoice.
/// </summary>
public class InvoiceItem : ITenantScopedEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the parent invoice.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the optional foreign key identifier of the linked product catalog item.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the line item description or service label.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity billed.
    /// </summary>
    public decimal Quantity { get; set; } = 1m;

    /// <summary>
    /// Gets or sets the unit price before tax and discounts.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the applied VAT tax rate percentage.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets line discount percentage (0 to 100).
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Gets or sets the calculated taxable line total after discount.
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure (e.g. EA, PZ, ORE).
    /// </summary>
    public string Uom { get; set; } = "EA";

    /// <summary>
    /// Gets or sets optional line notes or additional specifications.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the item was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the item was updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation reference to the parent invoice.
    /// </summary>
    public Invoice? Invoice { get; set; }

    /// <summary>
    /// Gets or sets the navigation reference to the product entity.
    /// </summary>
    public Product? Product { get; set; }
}
