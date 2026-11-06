// <copyright file="IssueInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Commands;

/// <summary>
/// Command to issue and finalize an invoice, locking it against modifications.
/// </summary>
/// <param name="InvoiceId">The unique identifier of the invoice to issue.</param>
public sealed record IssueInvoiceCommand(Guid InvoiceId) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesIssue;
}
