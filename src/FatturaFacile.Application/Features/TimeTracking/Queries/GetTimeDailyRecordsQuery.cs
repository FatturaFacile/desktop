// <copyright file="GetTimeDailyRecordsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Returns the daily records of an employee in a period.
/// </summary>
/// <param name="EmployeeId">The employee.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
public sealed record GetTimeDailyRecordsQuery(Guid EmployeeId, DateTime From, DateTime To) : IRequest<IReadOnlyList<TimeDailyRecord>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
