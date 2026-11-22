// <copyright file="GetTechnicianOfflineWorkQueueQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Reads the cached offline work bundle of a technician: the open work orders with customer and equipment, and the van stock count.
/// </summary>
/// <param name="TechnicianId">The technician identifier.</param>
public sealed record GetTechnicianOfflineWorkQueueQuery(string TechnicianId) : IRequest<FieldServiceCached<FieldServiceTechnicianBundle>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
