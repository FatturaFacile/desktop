// <copyright file="ConvertQuoteToOrderCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Command that turns an accepted quote into a draft sales order.
/// </summary>
/// <param name="QuoteId">The quote identifier.</param>
public sealed record ConvertQuoteToOrderCommand(Guid QuoteId) : IRequest<SalesOrderDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
