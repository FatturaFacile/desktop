// <copyright file="GetDropshipmentQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Reads one dropshipment with its virtual stock movements.
/// </summary>
/// <param name="DropshipmentId">The dropshipment.</param>
public sealed record GetDropshipmentQuery(
    Guid DropshipmentId) : IRequest<ProcurementLoad<DropshipmentView>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
