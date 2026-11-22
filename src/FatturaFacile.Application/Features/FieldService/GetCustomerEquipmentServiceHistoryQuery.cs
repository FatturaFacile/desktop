// <copyright file="GetCustomerEquipmentServiceHistoryQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Reads the cached service history of a customer asset.
/// </summary>
/// <param name="EquipmentId">The equipment identifier.</param>
public sealed record GetCustomerEquipmentServiceHistoryQuery(string EquipmentId) : IRequest<FieldServiceCached<EquipmentHistory>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
