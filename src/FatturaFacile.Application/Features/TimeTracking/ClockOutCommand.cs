// <copyright file="ClockOutCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Command to conclude an ongoing employee work shift and compute billable and total net hours worked.
/// </summary>
/// <param name="EmployeeId">The unique identifier of the clocking employee.</param>
/// <param name="BreakMinutes">The cumulative unbillable break duration taken during the shift, in minutes (defaults to 0).</param>
public record ClockOutCommand(
    Guid EmployeeId,
    int  BreakMinutes = 0) : IRequest<TimeEntryDto>;
