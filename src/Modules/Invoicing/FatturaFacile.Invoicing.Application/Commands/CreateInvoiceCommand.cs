// <copyright file="CreateInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Enums;

namespace FatturaFacile.Invoicing.Application.Commands;

/// <summary>
/// Command to create and issue a new multi-currency, multi-jurisdiction invoice.
/// </summary>
/// <param name="TenantId">The owning tenant identifier.</param>
/// <param name="ContactId">The recipient contact/customer identifier.</param>
/// <param name="InvoiceType">The invoice legal format and type.</param>
/// <param name="InvoiceNumber">The unique invoice reference number.</param>
/// <param name="Date">The invoice issue date.</param>
/// <param name="DueDate">The optional payment due date.</param>
/// <param name="Currency">The 3-letter ISO currency code.</param>
/// <param name="Jurisdiction">The legal and tax jurisdiction (IT or CH).</param>
/// <param name="Items">The list of line items to include.</param>
/// <param name="SplitPayment">Whether Italian split payment applies.</param>
/// <param name="ReverseCharge">Whether reverse charge applies.</param>
/// <param name="Notes">Optional notes or payment terms.</param>
/// <param name="ExchangeRate">The currency exchange rate.</param>
public sealed record CreateInvoiceCommand(
    Guid TenantId,
    Guid ContactId,
    InvoiceType InvoiceType,
    string InvoiceNumber,
    DateOnly Date,
    DateOnly? DueDate,
    string Currency,
    Jurisdiction Jurisdiction,
    List<CreateInvoiceItemInput> Items,
    bool SplitPayment = false,
    bool ReverseCharge = false,
    string? Notes = null,
    decimal ExchangeRate = 1.0m) : ICommand<InvoiceDto>;
