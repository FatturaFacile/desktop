// <copyright file="ExportEmployeeDataQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Exports all the personal data of an employee (right of access and portability: art. 25 and 28 LPD, art. 15 and 20 GDPR).
/// Employees can export their own data; other employees require the time or payroll reading rights.
/// </summary>
/// <param name="EmployeeId">The employee.</param>
public sealed record ExportEmployeeDataQuery(Guid EmployeeId) : IRequest<EmployeeDataExport>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.TimeClock;
}
