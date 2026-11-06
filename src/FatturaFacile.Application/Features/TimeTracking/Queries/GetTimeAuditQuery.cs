// <copyright file="GetTimeAuditQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Returns the audit trail of the working time records in a period.
/// </summary>
/// <param name="EmployeeId">The employee, or null for all.</param>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
public sealed record GetTimeAuditQuery(Guid? EmployeeId, DateTime From, DateTime To) : IRequest<IReadOnlyList<TimeAuditEntry>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeRead;
}
