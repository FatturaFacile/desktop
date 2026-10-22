// <copyright file="TimeEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.TimeTracking;

/// <summary>
/// Represents an employee clock-in/clock-out attendance and working time entry, supporting GPS geofencing, hardware badge recognition, and Swiss labor law compliance (ArG / OLL 1 detailed versus simplified recording).
/// </summary>
public sealed class TimeEntry : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
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
    /// Gets the UTC timestamp when the employee finished work (clocked out), or <see langword="null"/> if the work shift is currently active.
    /// </summary>
    public DateTime? ClockOutUtc { get; private set; }

    /// <summary>
    /// Gets the cumulative duration of unpaid breaks (lunch, personal pauses) taken during the shift, expressed in minutes.
    /// </summary>
    public int BreakDurationMinutes { get; private set; }

    /// <summary>
    /// Gets the GPS latitude coordinate where the clock-in event occurred, used for geofenced on-site attendance verification.
    /// </summary>
    public double? GpsLatitude { get; private set; }

    /// <summary>
    /// Gets the GPS longitude coordinate where the clock-in event occurred, used for geofenced on-site attendance verification.
    /// </summary>
    public double? GpsLongitude { get; private set; }

    /// <summary>
    /// Gets the unique hardware RFID/NFC badge serial number scanned at an on-premise attendance terminal, if applicable.
    /// </summary>
    public string? BadgeSerialNumber { get; private set; }

    /// <summary>
    /// Gets the Swiss Federal Act on Work in Industry, Trade and Commerce (ArG / OLL 1) compliance recording mode: Detailed time tracking versus Simplified tracking (Vereinfachte Zeiterfassung).
    /// </summary>
    public RecordingType RecordingType { get; private set; } = RecordingType.Detailed;

    /// <summary>
    /// Gets the net working hours performed during the shift rounded to two decimal places, calculated as the elapsed time between clock-in and clock-out minus any recorded break minutes; returns 0 if the shift is uncompleted or the duration is non-positive.
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
            return minutes > 0
                ? decimal.Round(
                    minutes / 60m,
                    2,
                    MidpointRounding.AwayFromZero)
                : 0m;
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
    /// Gets or sets the UTC timestamp when this time entry record was last updated, or <see langword="null"/> if never modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that last modified the time entry.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates and initializes a new <see cref="TimeEntry"/> instance recording an active clock-in event.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the employing tenant organization.</param>
    /// <param name="employeeId">The unique identifier of the employee clocking in.</param>
    /// <param name="clockInUtc">The UTC timestamp marking the shift commencement.</param>
    /// <param name="recordingType">The regulatory time tracking mode according to Swiss ArG / OLL 1 standards. Defaults to <see cref="RecordingType.Detailed"/>.</param>
    /// <param name="lat">The optional GPS latitude coordinate of the punch location.</param>
    /// <param name="lon">The optional GPS longitude coordinate of the punch location.</param>
    /// <param name="badgeSerial">The optional hardware badge or NFC card identifier.</param>
    /// <returns>A newly started <see cref="TimeEntry"/> entity.</returns>
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
    /// Records the clock-out event, concluding the active shift and deducting any unpaid break duration.
    /// </summary>
    /// <param name="clockOutUtc">The UTC timestamp marking the end of the shift.</param>
    /// <param name="breakMinutes">The cumulative minutes spent on unpaid breaks during the shift.</param>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="clockOutUtc"/> is chronologically earlier than <see cref="ClockInUtc"/>.</exception>
    public void ClockOut(
        DateTime clockOutUtc,
        int breakMinutes = 0)
    {
        if (clockOutUtc < ClockInUtc)
        {
            throw new InvalidOperationException("Clock out cannot be earlier than clock in.");
        }

        ClockOutUtc = clockOutUtc;
        BreakDurationMinutes = Math.Max(
            0,
            breakMinutes);
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
