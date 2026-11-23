// <copyright file="GetOrderRemindersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Reads the reminder log of an order.
/// </summary>
/// <param name="OrderId">The order.</param>
public sealed record GetOrderRemindersQuery(
    Guid OrderId) : IRequest<ProcurementLoad<IReadOnlyList<ReminderView>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
