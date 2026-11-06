// <copyright file="SendInvoiceToSdiCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Sdi;

/// <summary>
/// Sends an issued Italian invoice to the Sistema di Interscambio.
/// </summary>
/// <param name="InvoiceId">The invoice.</param>
public sealed record SendInvoiceToSdiCommand(Guid InvoiceId) : IRequest<string>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.InvoicesIssue;
}
