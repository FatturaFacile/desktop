using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Represents clock in command.
/// </summary>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="RecordingType">The recording type.</param>
/// <param name="Latitude">The latitude.</param>
/// <param name="Longitude">The longitude.</param>
/// <param name="BadgeSerial">The badge serial.</param>
public record ClockInCommand(
    Guid EmployeeId,
    RecordingType RecordingType = RecordingType.Detailed,
    double? Latitude = null,
    double? Longitude = null,
    string? BadgeSerial = null) : IRequest<TimeEntryDto>;
