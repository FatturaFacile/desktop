// <copyright file="TimeEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.TimeTracking;

/// <summary>
/// Represents an employee clock-in/clock-out attendance and working time entry,
/// supporting GPS geofencing, hardware badge recognition, and Swiss labor law compliance
/// (ArG / OLL 1 detailed versus simplified recording).
/// </summary>
public sealed class TimeEntry : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private const decimal ZeroHours = 0m;
    private const int ZeroMinutes = 0;
    private const int HoursPrecision = 2;
    private const decimal MinutesPerHour = 60m;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeEntry"/> class.
    /// </summary>
    private TimeEntry()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the employing tenant organization.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the employee recording attendance.
    /// </summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the employee commenced work (clocked in).
    /// </summary>
    public DateTime ClockInUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the employee finished work (clocked out),
    /// or <see langword="null"/> if the work shift is currently active.
    /// </summary>
    public DateTime? ClockOutUtc { get; private set; }

    /// <summary>
    /// Gets the cumulative duration of unpaid breaks (lunch, personal pauses)
    /// taken during the shift, expressed in minutes.
    /// </summary>
    public int BreakDurationMinutes { get; private set; }

    /// <summary>
    /// Gets the GPS latitude coordinate where the clock-in event occurred.
    /// </summary>
    public double? GpsLatitude { get; private set; }

    /// <summary>
    /// Gets the GPS longitude coordinate where the clock-in event occurred.
    /// </summary>
    public double? GpsLongitude { get; private set; }

    /// <summary>
    /// Gets the unique hardware RFID/NFC badge serial number scanned at an
    /// on-premise attendance terminal, if applicable.
    /// </summary>
    public string? BadgeSerialNumber { get; private set; }

    /// <summary>
    /// Gets the Swiss labor-law recording mode.
    /// </summary>
    public RecordingType RecordingType { get; private set; } = RecordingType.Detailed;

    /// <summary>
    /// Gets the net working hours performed during the shift rounded to two decimal places.
    /// </summary>
    public decimal TotalNetHours
    {
        get
        {
            if (!ClockOutUtc.HasValue)
            {
                return ZeroHours;
            }

            var workedMinutes =
                (decimal)(ClockOutUtc.Value - ClockInUtc).TotalMinutes
                - BreakDurationMinutes;

            return workedMinutes <= ZeroMinutes ? ZeroHours : decimal.Round( workedMinutes / MinutesPerHour, HoursPrecision, MidpointRounding.AwayFromZero);
        }
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the time entry record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or system terminal that recorded the clock-in event.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this time entry record was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that last modified the time entry.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates and initializes a new active time entry.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant organization.</param>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="clockInUtc">The UTC timestamp when the employee clocked in.</param>
    /// <param name="recordingType">The Swiss labor-law recording type.</param>
    /// <param name="lat">The GPS latitude coordinate where the clock-in event occurred.</param>
    /// <param name="lon">The GPS longitude coordinate where the clock-in event occurred.</param>
    /// <param name="badgeSerial">The unique hardware RFID/NFC badge serial number scanned at an on-premise attendance terminal.</param>
    /// <returns>A newly initialized <see cref="TimeEntry"/> instance.</returns>
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
    /// Records the employee clock-out event.
    /// </summary>
    /// <param name="clockOutUtc">Clock-out timestamp.</param>
    /// <param name="breakMinutes">Total unpaid break minutes.</param>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="clockOutUtc"/> is earlier than <see cref="ClockInUtc"/>.</exception>
    public void ClockOut(
        DateTime clockOutUtc,
        int breakMinutes = ZeroMinutes)
    {
        if (clockOutUtc < ClockInUtc)
        {
            throw new InvalidOperationException("Clock out cannot be earlier than clock in.");
        }

        ClockOutUtc = clockOutUtc;
        BreakDurationMinutes = Math.Max(ZeroMinutes, breakMinutes);
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
