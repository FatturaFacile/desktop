// <copyright file="CreateInvoiceLineItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Represents create invoice line item.
/// </summary>
/// <param name="Description">The description.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="UnitPrice">The unit price.</param>
/// <param name="VatRate">The vat rate.</param>
/// <param name="Natura">The natura.</param>
public record CreateInvoiceLineItem(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    string? Natura = null);
