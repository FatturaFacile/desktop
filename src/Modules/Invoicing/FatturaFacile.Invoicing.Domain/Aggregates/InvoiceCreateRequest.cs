// <copyright file="InvoiceCreateRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Invoicing.Domain.Enums;

namespace FatturaFacile.Invoicing.Domain.Aggregates;

/// <summary>
/// Represents the data required to create an invoice.
/// </summary>
/// <param name="TenantId">The unique identifier of the tenant.</param>
/// <param name="ContactId">The unique identifier of the contact associated with the invoice.</param>
/// <param name="InvoiceType">The type of the invoice (e.g., Standard, CreditNote).</param>
/// <param name="InvoiceNumber">The unique invoice number.</param>
/// <param name="Date">The date the invoice was issued.</param>
/// <param name="DueDate">The optional due date for payment.</param>
/// <param name="Currency">The 3-letter ISO currency code.</param>
/// <param name="Jurisdiction">The legal and tax jurisdiction (e.g., IT,
/// CH).</param>
/// <param name="SplitPayment">Indicates whether split payment applies (for IT jurisdiction).</param>
/// <param name="ReverseCharge">Indicates whether reverse charge applies (for IT jurisdiction).</param>
/// <param name="Notes">Optional notes or payment terms.</param>
/// <param name="ExchangeRate">The currency exchange rate to the tenant's base currency.</param>
public sealed record InvoiceCreateRequest(
    Guid TenantId,
    Guid ContactId,
    InvoiceType InvoiceType,
    string InvoiceNumber,
    DateOnly Date,
    DateOnly? DueDate,
    string Currency,
    Jurisdiction Jurisdiction,
    bool SplitPayment = false,
    bool ReverseCharge = false,
    string? Notes = null,
    decimal ExchangeRate = 1.0m);
