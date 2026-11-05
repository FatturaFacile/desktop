// <copyright file="InvoiceItemDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for an individual invoice line item.
/// </summary>
public sealed class InvoiceItemDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice foreign key identifier.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the linked product identifier.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the line description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the billed quantity.
    /// </summary>
    public decimal Quantity { get; set; } = 1m;

    /// <summary>
    /// Gets or sets the unit price before tax.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the VAT tax rate percentage.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the discount percentage.
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Gets or sets the calculated taxable line total.
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure.
    /// </summary>
    public string Uom { get; set; } = "EA";

    /// <summary>
    /// Gets or sets optional line notes.
    /// </summary>
    public string? Notes { get; set; }
}
