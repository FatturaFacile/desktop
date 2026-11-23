// <copyright file="CreateLocalHolidayCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Adds a local holiday of the tenant.
/// </summary>
/// <param name="Holiday">The holiday.</param>
public sealed record CreateLocalHolidayCommand(LocalHolidayRequest Holiday) : IRequest<PlanningCommandOutcome<LocalHoliday>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
