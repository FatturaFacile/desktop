// <copyright file="ClockInCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;

using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Command to begin and record a new active work shift session for an employee.
/// </summary>
/// <param name="EmployeeId">The unique identifier of the clocking employee.</param>
/// <param name="RecordingType">The medium or device type used for recording (e.g., Detailed, Simplified, Mobile).</param>
/// <param name="Latitude">Optional geographic latitude coordinate captured at clock-in.</param>
/// <param name="Longitude">Optional geographic longitude coordinate captured at clock-in.</param>
/// <param name="BadgeSerial">The optional hardware RFID or NFC badge serial number used at a terminal.</param>
public record ClockInCommand(
    Guid          EmployeeId,
    RecordingType RecordingType = RecordingType.Detailed,
    double?       Latitude      = null,
    double?       Longitude     = null,
    string?       BadgeSerial   = null) : IRequest<TimeEntryDto>;
