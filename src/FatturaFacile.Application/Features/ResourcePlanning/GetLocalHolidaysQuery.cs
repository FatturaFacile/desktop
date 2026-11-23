// <copyright file="GetLocalHolidaysQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the local holidays of the tenant.
/// </summary>
public sealed record GetLocalHolidaysQuery : IRequest<IReadOnlyList<LocalHoliday>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
