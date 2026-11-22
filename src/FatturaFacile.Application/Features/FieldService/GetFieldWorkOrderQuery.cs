// <copyright file="GetFieldWorkOrderQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Reads a cached work order with its parts and service report, with the operations still in the outbox laid over it (marked pending). It never calls the server.
/// </summary>
/// <param name="WorkOrderId">The work order identifier.</param>
public sealed record GetFieldWorkOrderQuery(string WorkOrderId) : IRequest<FieldServiceCached<FieldServiceWorkOrder>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
