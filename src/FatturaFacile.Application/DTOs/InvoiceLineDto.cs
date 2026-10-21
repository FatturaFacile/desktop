// <copyright file="InvoiceLineDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents invoice line dto.
/// </summary>
/// <param name="LineNumber">The line number.</param>
/// <param name="Description">The description.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="UnitPrice">The unit price.</param>
/// <param name="VatRate">The vat rate.</param>
/// <param name="NaturaExemptionCode">The natura exemption code.</param>
/// <param name="LineTotal">The line total.</param>
public record InvoiceLineDto(
    int LineNumber,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    string? NaturaExemptionCode,
    decimal LineTotal);
