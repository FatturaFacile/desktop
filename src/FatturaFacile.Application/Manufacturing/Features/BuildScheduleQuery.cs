// <copyright file="BuildScheduleQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Manufacturing.Scheduling;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Builds the local finite-capacity schedule of the open production orders (offline).
/// </summary>
/// <param name="HorizonStart">The earliest instant an operation may start.</param>
public sealed record BuildScheduleQuery(DateTime HorizonStart) : IRequest<ScheduleResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.ManufacturingRead;
}
