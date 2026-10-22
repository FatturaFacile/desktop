// <copyright file="TimeEntryDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an employee work shift attendance and time entry.
/// </summary>
/// <param name="Id">The unique time entry record identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this time entry.</param>
/// <param name="EmployeeId">The unique identifier of the recording employee.</param>
/// <param name="EmployeeName">The display full name of the employee.</param>
/// <param name="ClockInUtc">The UTC date and time when the employee commenced work.</param>
/// <param name="ClockOutUtc">The UTC date and time when work concluded, or <see langword="null"/> if the shift is currently active.</param>
/// <param name="BreakDurationMinutes">The cumulative unbillable break time recorded, in minutes.</param>
/// <param name="TotalNetHours">The calculated net payable working hours deducting breaks.</param>
/// <param name="RecordingType">The medium or channel through which time was captured (e.g., Badge, Mobile, Web).</param>
/// <param name="GpsLatitude">Optional geographic latitude coordinate recorded during clock-in.</param>
/// <param name="GpsLongitude">Optional geographic longitude coordinate recorded during clock-in.</param>
public record TimeEntryDto(
    Guid Id,
    Guid TenantId,
    Guid EmployeeId,
    string EmployeeName,
    DateTime ClockInUtc,
    DateTime? ClockOutUtc,
    int BreakDurationMinutes,
    decimal TotalNetHours,
    RecordingType RecordingType,
    double? GpsLatitude,
    double? GpsLongitude);
