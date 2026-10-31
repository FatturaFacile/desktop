// <copyright file="CreateInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Command request to generate and persist a new billing document (invoice, proforma, credit note) in the active tenant.
/// </summary>
/// <param name="ContactId">The unique identifier of the recipient customer contact.</param>
/// <param name="Date">The document issue date.</param>
/// <param name="DueDate">The invoice payment due date.</param>
/// <param name="Currency">The three-letter ISO currency code (e.g. "EUR", "CHF").</param>
/// <param name="InvoiceType">The fiscal document type classification.</param>
/// <param name="Notes">Optional commercial notes, payment instructions, or customer remarks.</param>
/// <param name="BankName">The banking institution name for remittance.</param>
/// <param name="BankIban">The standard IBAN account string for direct bank transfer.</param>
/// <param name="QrIban">The special QR-IBAN used for Swiss QR-Bill remittance if applicable.</param>
/// <param name="Items">The itemized collection of billable goods and services.</param>
public record CreateInvoiceCommand(
    Guid ContactId,
    DateTime Date,
    DateTime DueDate,
    string Currency,
    InvoiceType InvoiceType,
    string? Notes,
    string? BankName,
    string? BankIban,
    string? QrIban,
    IReadOnlyList<CreateInvoiceLineItemCommand> Items) : IRequest<InvoiceDto>, ICommand<InvoiceDto>;
