using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

public record ClockInCommand(
    Guid EmployeeId,
    RecordingType RecordingType = RecordingType.Detailed,
    double? Latitude = null,
    double? Longitude = null,
    string? BadgeSerial = null) : IRequest<TimeEntryDto>;