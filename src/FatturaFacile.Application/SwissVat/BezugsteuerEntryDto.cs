// <copyright file="BezugsteuerEntryDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Data transfer object for a Swiss Bezugsteuer entry.
/// </summary>
/// <param name="Id">Entry identifier.</param>
/// <param name="SupplierName">Foreign supplier name.</param>
/// <param name="SupplierCountry">Supplier ISO 3166-1 alpha-2 country code.</param>
/// <param name="InvoiceNumber">Supplier invoice number.</param>
/// <param name="InvoiceDate">Invoice date.</param>
/// <param name="ServiceCategory">Service category.</param>
/// <param name="Description">Service description.</param>
/// <param name="NetAmountChf">Net amount in CHF.</param>
/// <param name="VatRatePercent">Applied Swiss VAT rate percentage.</param>
/// <param name="TaxAmountChf">Computed Bezugsteuer amount in CHF.</param>
/// <param name="DeductibleInputTaxChf">Deductible input tax in CHF.</param>
/// <param name="AfcBox">Target AFC box code (382 or 383).</param>
/// <param name="Status">Review status.</param>
public sealed record BezugsteuerEntryDto(
    Guid Id,
    string SupplierName,
    string SupplierCountry,
    string InvoiceNumber,
    DateOnly InvoiceDate,
    BezugsteuerCategory ServiceCategory,
    string Description,
    decimal NetAmountChf,
    decimal VatRatePercent,
    decimal TaxAmountChf,
    decimal DeductibleInputTaxChf,
    string AfcBox,
    BezugsteuerStatus Status);
