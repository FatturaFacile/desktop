// <copyright file="CreateInvoiceLineItemCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Sub-command payload specifying a single line item to include within a new invoice creation request.
/// </summary>
/// <param name="ProductId">The optional product identifier from the tenant catalog.</param>
/// <param name="Description">The line item description of goods or services provided.</param>
/// <param name="Quantity">The quantity of units billed.</param>
/// <param name="UnitPrice">The net price per unit before line discount.</param>
/// <param name="TaxRate">The applicable VAT/tax percentage rate.</param>
/// <param name="DiscountPercentage">The percentage discount to deduct from the line item net total.</param>
public record CreateInvoiceLineItemCommand(
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountPercentage);
