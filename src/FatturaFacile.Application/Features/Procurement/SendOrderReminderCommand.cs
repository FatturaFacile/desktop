// <copyright file="SendOrderReminderCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Reminds a supplier of an overdue order offline-first.
/// </summary>
/// <param name="Request">The request.</param>
public sealed record SendOrderReminderCommand(
    OrderReminderRequest Request) : IRequest<ProcurementActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
