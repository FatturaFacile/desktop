// <copyright file="SaveTimeEventCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Creates or corrects a clock event (manager); the reason is mandatory and kept in the audit trail.
/// </summary>
/// <param name="Event">The event values.</param>
/// <param name="Reason">The reason of the correction.</param>
public sealed record SaveTimeEventCommand(TimeClockEvent Event, string Reason) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeManage;
}
