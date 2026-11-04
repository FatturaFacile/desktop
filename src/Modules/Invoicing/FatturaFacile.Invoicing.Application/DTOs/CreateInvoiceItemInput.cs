// <copyright file="CreateInvoiceItemInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Invoicing.Application.DTOs;

/// <summary>
/// Input parameters required to construct an invoice line item during invoice creation.
/// </summary>
/// <param name="ProductId">The optional product catalog identifier.</param>
/// <param name="Description">The line item description.</param>
/// <param name="Quantity">The quantity supplied.</param>
/// <param name="UnitPrice">The net unit price.</param>
/// <param name="TaxRate">The VAT percentage rate.</param>
/// <param name="DiscountPercentage">The discount percentage (default: 0).</param>
public sealed record CreateInvoiceItemInput(
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountPercentage = 0m);
