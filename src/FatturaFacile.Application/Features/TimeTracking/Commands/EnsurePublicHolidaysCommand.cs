// <copyright file="EnsurePublicHolidaysCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Creates the default public holidays of a year for the company canton when the year has none.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record EnsurePublicHolidaysCommand(int Year) : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeManage;
}
