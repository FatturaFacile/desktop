// <copyright file="InvoiceLineDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an individual commercial item line within an invoice document.
/// </summary>
/// <param name="LineNumber">The positive ordinal position sequence number of the line within the document.</param>
/// <param name="Description">The textual description of the goods delivered or services rendered.</param>
/// <param name="Quantity">The quantity of items or service units billed.</param>
/// <param name="UnitPrice">The net unit price excluding value-added tax.</param>
/// <param name="VatRate">The percentage rate of value-added tax applicable to this line.</param>
/// <param name="NaturaExemptionCode">The Italian FatturaPA VAT exemption nature code (e.g., "N1", "N2.2"), if zero-rated or exempt.</param>
/// <param name="LineTotal">The computed net total amount for this invoice line (quantity multiplied by unit price less discounts).</param>
public record InvoiceLineDto(
    int     LineNumber,
    string  Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    string? NaturaExemptionCode,
    decimal LineTotal);
