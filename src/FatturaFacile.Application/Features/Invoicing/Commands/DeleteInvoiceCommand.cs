// <copyright file="DeleteInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Commands;

/// <summary>
/// Command to delete a draft invoice.
/// </summary>
/// <param name="InvoiceId">The identifier of the invoice to delete.</param>
public sealed record DeleteInvoiceCommand(Guid InvoiceId) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
