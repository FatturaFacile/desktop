// <copyright file="CancelInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Invoicing.Application.DTOs;

namespace FatturaFacile.Invoicing.Application.Commands;

/// <summary>
/// Command to void or cancel an outstanding unpaid invoice.
/// </summary>
/// <param name="TenantId">The owning tenant identifier.</param>
/// <param name="InvoiceId">The unique identifier of the invoice to cancel.</param>
/// <param name="Reason">The cancellation justification or rationale.</param>
public sealed record CancelInvoiceCommand(
    Guid   TenantId,
    Guid   InvoiceId,
    string Reason) : ICommand<InvoiceDto>;
