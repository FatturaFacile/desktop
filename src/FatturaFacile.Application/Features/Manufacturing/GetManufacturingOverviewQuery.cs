// <copyright file="GetManufacturingOverviewQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Manufacturing;

/// <summary>
/// Query that reads the cached manufacturing data (work centres, machines, calendars, routings and OEE) without a connection.
/// </summary>
public sealed record GetManufacturingOverviewQuery : IRequest<ManufacturingOverview>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
