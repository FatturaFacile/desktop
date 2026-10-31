// <copyright file="InvoiceDetailDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an invoice containing comprehensive fiscal details and all item lines.
/// </summary>
/// <param name="Id">The unique invoice entity identifier.</param>
/// <param name="TenantId">The organizational tenant boundary identifier.</param>
/// <param name="ContactId">The recipient contact entity identifier.</param>
/// <param name="ContactName">The commercial or legal name of the recipient party.</param>
/// <param name="InvoiceType">The fiscal document type classification (e.g., Standard, CreditNote).</param>
/// <param name="InvoiceNumber">The formatted unique document sequence identifier.</param>
/// <param name="FiscalYear">The accounting fiscal year associated with the document.</param>
/// <param name="SequentialNumber">The ordinal chronological sequence number within the fiscal year.</param>
/// <param name="Date">The official accounting issuance date.</param>
/// <param name="DueDate">The agreed payment due date.</param>
/// <param name="Currency">The three-letter ISO currency code.</param>
/// <param name="ExchangeRate">The currency exchange rate relative to the tenant's base currency.</param>
/// <param name="TotalNet">The aggregated net taxable subtotal.</param>
/// <param name="TotalTax">The calculated total value-added tax.</param>
/// <param name="TotalGross">The total gross invoice amount including tax.</param>
/// <param name="RoundingAmount">The fractional rounding difference applied to the gross sum.</param>
/// <param name="TotalPaid">The cumulative settled or reconciled payments against this invoice.</param>
/// <param name="PaymentStatus">The current workflow payment status.</param>
/// <param name="Notes">Free-form customer-facing or internal remarks.</param>
/// <param name="BankName">The remitting bank financial institution name.</param>
/// <param name="BankIban">The remitting bank International Bank Account Number.</param>
/// <param name="QrIban">The Swiss QR-IBAN account number for Swiss QR-bill transactions.</param>
/// <param name="Items">The detailed read-only collection of invoice line items.</param>
public record InvoiceDetailDto(
    Guid Id,
    Guid TenantId,
    Guid ContactId,
    string ContactName,
    string InvoiceType,
    string InvoiceNumber,
    int FiscalYear,
    int SequentialNumber,
    DateTime Date,
    DateTime DueDate,
    string Currency,
    decimal ExchangeRate,
    decimal TotalNet,
    decimal TotalTax,
    decimal TotalGross,
    decimal RoundingAmount,
    decimal TotalPaid,
    string PaymentStatus,
    string? Notes,
    string? BankName,
    string? BankIban,
    string? QrIban,
    IReadOnlyList<InvoiceItemDto> Items)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceDetailDto"/> record with default empty values.
    /// </summary>
    public InvoiceDetailDto()
        : this(
            default,
            default,
            default,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            0,
            default,
            default,
            string.Empty,
            1.0m,
            0,
            0,
            0,
            0,
            0,
            string.Empty,
            null,
            null,
            null,
            null,
            Array.Empty<InvoiceItemDto>())
    {
    }
}
