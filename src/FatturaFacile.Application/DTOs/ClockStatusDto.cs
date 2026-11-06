// <copyright file="ClockStatusDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// The clock status of the employee linked to the current user.
/// </summary>
/// <param name="EmployeeId">The employee identifier.</param>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="State">Off, Working or OnBreak.</param>
/// <param name="LastEvent">The time of the last clock event.</param>
/// <param name="WorkedTodayMinutes">The minutes worked today (until now when working).</param>
public sealed record ClockStatusDto(Guid EmployeeId, string EmployeeName, string State, DateTime? LastEvent, int WorkedTodayMinutes);
