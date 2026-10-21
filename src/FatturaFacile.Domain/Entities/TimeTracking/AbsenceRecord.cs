// <copyright file="AbsenceRecord.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.TimeTracking;

/// <summary>
/// Represents absence record.
/// </summary>
public sealed class AbsenceRecord : Entity<Guid>, ITenantEntity
{
    private AbsenceRecord()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the employee id.
    /// </summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// Gets the absence type.
    /// </summary>
    public AbsenceType AbsenceType { get; private set; }

    /// <summary>
    /// Gets the start date.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the end date.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Gets the total working days.
    /// </summary>
    public decimal TotalWorkingDays { get; private set; }

    /// <summary>
    /// Gets a value indicating whether approved.
    /// </summary>
    public bool IsApproved { get; private set; }

    /// <summary>
    /// Gets the iT: INPS PUC Protocollo Telematico; CH: KTG Arztzeugnis / APG-EO Meldekarte.
    /// </summary>
    public string? OfficialCertificateNumber { get; private set; }

    /// <summary>
    /// Gets the notes.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="employeeId">The employee id.</param>
    /// <param name="type">The type.</param>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <param name="days">The days.</param>
    /// <param name="certificateNumber">The certificate number.</param>
    /// <param name="notes">The notes.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static AbsenceRecord Create(
        Guid tenantId,
        Guid employeeId,
        AbsenceType type,
        DateTime start,
        DateTime end,
        decimal days,
        string? certificateNumber = null,
        string? notes = null)
    {
        return new AbsenceRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            AbsenceType = type,
            StartDate = start,
            EndDate = end,
            TotalWorkingDays = days,
            OfficialCertificateNumber = certificateNumber,
            Notes = notes,
            IsApproved = false,
        };
    }

    /// <summary>
    /// Approve.
    /// </summary>
    public void Approve() => IsApproved = true;
}
