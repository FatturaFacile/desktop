// <copyright file="SavePurchaseInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Purchasing;

/// <summary>
/// Creates or updates a draft supplier invoice with its lines.
/// </summary>
/// <param name="Invoice">The header values.</param>
/// <param name="Lines">The lines.</param>
public sealed record SavePurchaseInvoiceCommand(PurchaseInvoice Invoice, IReadOnlyList<PurchaseInvoiceLine> Lines) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
