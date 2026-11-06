// <copyright file="PostPurchaseInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Purchasing;

/// <summary>
/// Posts a supplier invoice to the general ledger (expenses and input VAT against payables).
/// </summary>
/// <param name="Id">The supplier invoice identifier.</param>
public sealed record PostPurchaseInvoiceCommand(Guid Id) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
