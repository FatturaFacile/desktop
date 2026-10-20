using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

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