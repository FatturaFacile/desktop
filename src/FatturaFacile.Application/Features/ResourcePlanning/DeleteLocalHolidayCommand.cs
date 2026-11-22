// <copyright file="DeleteLocalHolidayCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Removes a local holiday of the tenant.
/// </summary>
/// <param name="HolidayId">The holiday.</param>
public sealed record DeleteLocalHolidayCommand(Guid HolidayId) : IRequest<PlanningCommandOutcome<bool>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
