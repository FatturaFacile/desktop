using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents time entry dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="ClockInUtc">The clock in utc.</param>
/// <param name="ClockOutUtc">The clock out utc.</param>
/// <param name="BreakDurationMinutes">The break duration minutes.</param>
/// <param name="TotalNetHours">The total net hours.</param>
/// <param name="RecordingType">The recording type.</param>
/// <param name="GpsLatitude">The gps latitude.</param>
/// <param name="GpsLongitude">The gps longitude.</param>
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
