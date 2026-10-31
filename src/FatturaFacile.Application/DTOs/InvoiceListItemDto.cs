// <copyright file="InvoiceListItemDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an invoice record summarized for data grid and list views.
/// </summary>
/// <param name="Id">The unique invoice entity identifier.</param>
/// <param name="TenantId">The organizational tenant boundary identifier.</param>
/// <param name="InvoiceNumber">The formatted unique document sequence number.</param>
/// <param name="Date">The official accounting issuance date of the invoice.</param>
/// <param name="DueDate">The settlement due date for payment completion.</param>
/// <param name="CustomerName">The formatted recipient customer or company name.</param>
/// <param name="Currency">The three-letter ISO currency code of the document.</param>
/// <param name="TotalNet">The aggregated taxable base amount excluding tax.</param>
/// <param name="TotalTax">The calculated total value-added tax amount.</param>
/// <param name="TotalGross">The total invoice gross amount due including tax.</param>
/// <param name="PaymentStatus">The current workflow payment settlement state.</param>
/// <param name="ItemsCount">The total count of distinct billable line items.</param>
public record InvoiceListItemDto(
    Guid Id,
    Guid TenantId,
    string InvoiceNumber,
    DateTime Date,
    DateTime DueDate,
    string CustomerName,
    string Currency,
    decimal TotalNet,
    decimal TotalTax,
    decimal TotalGross,
    string PaymentStatus,
    int ItemsCount)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceListItemDto"/> record with default empty values.
    /// </summary>
    public InvoiceListItemDto()
        : this(default, default, string.Empty, default, default, string.Empty, string.Empty, 0, 0, 0, string.Empty, 0)
    {
    }
}
