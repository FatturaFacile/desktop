// <copyright file="InvoiceItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a single billable line item within an invoice document.
/// </summary>
public class InvoiceItem : EntityBase, IMustHaveTenant
{
    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the parent invoice document.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the parent invoice.
    /// </summary>
    public Invoice? Invoice { get; set; }

    /// <summary>
    /// Gets or sets the optional foreign key identifier referencing a product catalog entry.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the textual description of the billed good or service.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the billed quantity of units.
    /// </summary>
    public decimal Quantity { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the unit price before discounts and taxes.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the applicable VAT tax percentage rate.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the percentage discount applied directly to this line item.
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Gets or sets the calculated net line total after deducting line discounts.
    /// </summary>
    public decimal LineTotal { get; set; }
}
