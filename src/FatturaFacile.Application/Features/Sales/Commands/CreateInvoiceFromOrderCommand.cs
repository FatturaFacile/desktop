// <copyright file="CreateInvoiceFromOrderCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Command that creates a draft invoice from a confirmed sales order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record CreateInvoiceFromOrderCommand(Guid OrderId) : IRequest<InvoiceDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
