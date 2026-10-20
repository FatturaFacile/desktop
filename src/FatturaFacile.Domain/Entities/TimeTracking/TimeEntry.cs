using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.TimeTracking;

/// <summary>
/// Represents time entry.
/// </summary>
public sealed class TimeEntry : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the employee id.
    /// </summary>
    public Guid EmployeeId { get; private set; }

/// <summary>
    /// Gets the clock in utc.
    /// </summary>
    public DateTime ClockInUtc { get; private set; }

/// <summary>
    /// Gets the clock out utc.
    /// </summary>
    public DateTime? ClockOutUtc { get; private set; }

/// <summary>
    /// Gets the break duration minutes.
    /// </summary>
    public int BreakDurationMinutes { get; private set; }

/// <summary>
    /// Gets the gPS Geofence &amp; Hardware Badge tracking.
    /// </summary>
    public double? GpsLatitude { get; private set; }

/// <summary>
    /// Gets the gps longitude.
    /// </summary>
    public double? GpsLongitude { get; private set; }

/// <summary>
    /// Gets the badge serial number.
    /// </summary>
    public string? BadgeSerialNumber { get; private set; }

/// <summary>
    /// Gets the swiss ArG / OLL 1 compliance mode: Detailed vs Simplified (Vereinfachte Zeiterfassung).
    /// </summary>
    public RecordingType RecordingType { get; private set; } = RecordingType.Detailed;

/// <summary>
    /// Gets the total Hours.
    /// </summary>
    public decimal TotalNetHours
    {
        get
        {
            if (!ClockOutUtc.HasValue)
            {
                return 0m;
            }

            var span = ClockOutUtc.Value - ClockInUtc;
            var minutes = (decimal)span.TotalMinutes - BreakDurationMinutes;
            return minutes > 0 ? decimal.Round(minutes / 60m, 2, MidpointRounding.AwayFromZero) : 0m;
        }
    }

/// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

private TimeEntry()
    {
    }

/// <summary>
    /// Clock In.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="employeeId">The employee id.</param>
    /// <param name="clockInUtc">The clock in utc.</param>
    /// <param name="recordingType">The recording type.</param>
    /// <param name="lat">The lat.</param>
    /// <param name="lon">The lon.</param>
    /// <param name="badgeSerial">The badge serial.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static TimeEntry ClockIn(
        Guid tenantId,
        Guid employeeId,
        DateTime clockInUtc,
        RecordingType recordingType = RecordingType.Detailed,
        double? lat = null,
        double? lon = null,
        string? badgeSerial = null)
    {
        return new TimeEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            ClockInUtc = clockInUtc,
            RecordingType = recordingType,
            GpsLatitude = lat,
            GpsLongitude = lon,
            BadgeSerialNumber = badgeSerial,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

/// <summary>
    /// Clock Out.
    /// </summary>
    /// <param name="clockOutUtc">The clock out utc.</param>
    /// <param name="breakMinutes">The break minutes.</param>
    /// <exception cref="InvalidOperationException">Thrown when clock out timestamp is earlier than clock in.</exception>
    public void ClockOut(DateTime clockOutUtc, int breakMinutes = 0)
    {
        if (clockOutUtc < ClockInUtc)
        {
            throw new InvalidOperationException("Clock out cannot be earlier than clock in.");
        }

        ClockOutUtc = clockOutUtc;
        BreakDurationMinutes = Math.Max(0, breakMinutes);
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
