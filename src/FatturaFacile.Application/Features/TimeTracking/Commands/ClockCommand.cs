// <copyright file="ClockCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Clocks the employee linked to the current user (entry, exit, break) at the current time.
/// </summary>
/// <param name="EventType">In, Out, BreakStart or BreakEnd.</param>
public sealed record ClockCommand(string EventType) : IRequest<ClockStatusDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
