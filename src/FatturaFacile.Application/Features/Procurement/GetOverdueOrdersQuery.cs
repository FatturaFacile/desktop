// <copyright file="GetOverdueOrdersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Reads the open orders past their promised delivery date.
/// </summary>
public sealed record GetOverdueOrdersQuery : IRequest<ProcurementLoad<IReadOnlyList<OverdueOrderView>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
