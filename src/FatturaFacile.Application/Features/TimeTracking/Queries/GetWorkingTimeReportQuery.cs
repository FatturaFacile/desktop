// <copyright file="GetWorkingTimeReportQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Evaluates the working time of an employee in a period against the Labour Act.
/// </summary>
/// <param name="EmployeeId">The employee.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
public sealed record GetWorkingTimeReportQuery(Guid EmployeeId, DateTime From, DateTime To) : IRequest<WorkingTimeReport>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
