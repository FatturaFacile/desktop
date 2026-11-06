// <copyright file="GetTimeEventsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Returns the clock events of an employee in a period (deleted events included on request, for the audit).
/// </summary>
/// <param name="EmployeeId">The employee.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="IncludeDeleted">Whether deleted events are returned.</param>
public sealed record GetTimeEventsQuery(Guid EmployeeId, DateTime From, DateTime To, bool IncludeDeleted = false) : IRequest<IReadOnlyList<TimeClockEvent>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
