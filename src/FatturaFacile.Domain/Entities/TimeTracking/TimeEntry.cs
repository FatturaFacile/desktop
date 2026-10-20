using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.TimeTracking;

public sealed class TimeEntry : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; private set; }
    public DateTime ClockInUtc { get; private set; }
    public DateTime? ClockOutUtc { get; private set; }
    public int BreakDurationMinutes { get; private set; }

    /// <summary>
    /// GPS Geofence & Hardware Badge tracking
    /// </summary>
    public double? GpsLatitude { get; private set; }
    public double? GpsLongitude { get; private set; }
    public string? BadgeSerialNumber { get; private set; }

    /// <summary>
    /// Swiss ArG / OLL 1 compliance mode: Detailed vs Simplified (Vereinfachte Zeiterfassung)
    /// </summary>
    public RecordingType RecordingType { get; private set; } = RecordingType.Detailed;

    /// <summary>
    /// Total Hours
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

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private TimeEntry()
    {
    }

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