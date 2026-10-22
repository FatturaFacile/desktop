// <copyright file="CreateInvoiceLineItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Command parameter model representing an individual invoice item or billing line to add during invoice creation.
/// </summary>
/// <param name="Description">The descriptive line item title detailing goods sold or services performed.</param>
/// <param name="Quantity">The quantity of billing units.</param>
/// <param name="UnitPrice">The net unit price before value-added tax.</param>
/// <param name="VatRate">The applicable VAT percentage rate (e.g., 22.0, 10.0, 4.0, 0.0).</param>
/// <param name="Natura">The Italian electronic invoice VAT exemption nature code (e.g., "N1", "N2.2", "N4"), or <see langword="null"/> if standard taxable.</param>
public record CreateInvoiceLineItem(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    string? Natura = null);
