// <copyright file="RegisterInvoicePaymentCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Invoicing.Application.DTOs;

namespace FatturaFacile.Invoicing.Application.Commands;

/// <summary>
/// Command to record a monetary payment settlement against an invoice.
/// </summary>
/// <param name="TenantId">The owning tenant identifier.</param>
/// <param name="InvoiceId">The unique identifier of the invoice being paid.</param>
/// <param name="Amount">The payment amount received.</param>
/// <param name="PaidAt">The UTC timestamp of the payment.</param>
public sealed record RegisterInvoicePaymentCommand(
    Guid           TenantId,
    Guid           InvoiceId,
    decimal        Amount,
    DateTimeOffset PaidAt) : ICommand<InvoiceDto>;
