// <copyright file="ChangeSalesOrderStatusCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Sales;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Command that moves a sales order through its workflow (confirm, approve, hold, ship, complete, cancel).
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="TargetStatus">The target status, one of <see cref="SalesOrderStatuses"/>.</param>
public sealed record ChangeSalesOrderStatusCommand(Guid OrderId, string TargetStatus) : IRequest<SalesOrderDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission =>
        string.Equals(TargetStatus, SalesOrderStatuses.Approved, StringComparison.Ordinal) ? Permission.InvoicesIssue : Permission.InvoicesWrite;
}
