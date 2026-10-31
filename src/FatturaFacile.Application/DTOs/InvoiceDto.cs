// <copyright file="InvoiceDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an invoice entity used across service boundaries and view projections.
/// </summary>
/// <param name="Id">The unique invoice entity identifier.</param>
/// <param name="TenantId">The organizational tenant boundary identifier.</param>
/// <param name="ContactId">The recipient customer contact identifier.</param>
/// <param name="ContactName">The commercial or legal name of the recipient party.</param>
/// <param name="InvoiceType">The document type classification (e.g., Standard, Proforma).</param>
/// <param name="InvoiceNumber">The formatted unique document number.</param>
/// <param name="FiscalYear">The accounting fiscal year.</param>
/// <param name="SequentialNumber">The ordinal sequence number within the fiscal year.</param>
/// <param name="Date">The official accounting issuance date.</param>
/// <param name="DueDate">The settlement due date.</param>
/// <param name="Currency">The three-letter ISO currency code.</param>
/// <param name="ExchangeRate">The conversion rate to tenant base currency.</param>
/// <param name="TotalNet">The net taxable total amount.</param>
/// <param name="TotalTax">The calculated total value-added tax amount.</param>
/// <param name="TotalGross">The total gross amount due.</param>
/// <param name="RoundingAmount">The fractional rounding applied to the total.</param>
/// <param name="TotalPaid">The settled or reconciled amount.</param>
/// <param name="PaymentStatus">The current workflow payment status.</param>
/// <param name="Notes">Optional descriptive remarks.</param>
/// <param name="BankName">The financial institution name for remittance.</param>
/// <param name="BankIban">The recipient bank IBAN.</param>
/// <param name="QrIban">The Swiss QR-IBAN account number.</param>
/// <param name="ItemsCount">The count of line items.</param>
/// <param name="Items">The collection of billable item records.</param>
public record InvoiceDto(
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
    int ItemsCount,
    IReadOnlyList<InvoiceItemDto> Items)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceDto"/> record with default empty values.
    /// </summary>
    public InvoiceDto()
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
            0,
            Array.Empty<InvoiceItemDto>())
    {
    }
}
