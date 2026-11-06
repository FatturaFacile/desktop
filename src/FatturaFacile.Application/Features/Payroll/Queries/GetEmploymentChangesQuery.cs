// <copyright file="GetEmploymentChangesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Returns the entries and exits of a month that must be notified to the insurers and to the withholding tax authority.
/// </summary>
/// <param name="Year">The year.</param>
/// <param name="Month">The month.</param>
public sealed record GetEmploymentChangesQuery(int Year, int Month) : IRequest<IReadOnlyList<EmploymentChangeDto>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollRead;
}
