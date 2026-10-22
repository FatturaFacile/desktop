// <copyright file="AbsenceRecord.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.TimeTracking;

/// <summary>
/// Represents an employee absence or statutory leave entitlement record
/// (such as vacation/Ferie, accrued leave/ROL, illness, or maternity),
/// maintaining medical certificate tracking across Italian (INPS) and Swiss (KTG/APG-EO) systems.
/// </summary>
public sealed class AbsenceRecord : Entity<Guid>, ITenantEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="AbsenceRecord"/> class.
    /// </summary>
    private AbsenceRecord()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization employing the individual.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the employee requesting or taking the leave.
    /// </summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// Gets the category of absence taken (e.g., Vacation, Sick Leave, Parental Leave, Training).
    /// </summary>
    public AbsenceType AbsenceType { get; private set; }

    /// <summary>
    /// Gets the starting calendar date of the absence period.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the inclusive ending calendar date of the absence period.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Gets the cumulative count of contractual working days or fractional days deducted for this absence.
    /// </summary>
    public decimal TotalWorkingDays { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this absence request has received managerial or human resources approval.
    /// </summary>
    public bool IsApproved { get; private set; }

    /// <summary>
    /// Gets the statutory certificate registration protocol number (e.g., Italian INPS PUC electronic sick leave certificate protocol number, or Swiss KTG Arztzeugnis / APG-EO Meldekarte reference).
    /// </summary>
    public string? OfficialCertificateNumber { get; private set; }

    /// <summary>
    /// Gets internal justification notes, HR remarks, or medical leave comments.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Creates and initializes a new <see cref="AbsenceRecord"/> entry in an initial unapproved state.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the employing tenant organization.</param>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="type">The classification category of the absence.</param>
    /// <param name="start">The start date of the absence interval.</param>
    /// <param name="end">The end date of the absence interval.</param>
    /// <param name="days">The calculated total working days encompassed by the leave.</param>
    /// <param name="certificateNumber">The optional official medical or statutory certificate identifier.</param>
    /// <param name="notes">Optional remarks or operational details regarding the absence.</param>
    /// <returns>A newly created <see cref="AbsenceRecord"/> instance awaiting approval.</returns>
    public static AbsenceRecord Create(
        Guid        tenantId,
        Guid        employeeId,
        AbsenceType type,
        DateTime    start,
        DateTime    end,
        decimal     days,
        string?     certificateNumber = null,
        string?     notes             = null)
    {
        return new AbsenceRecord
               {
                   Id                        = Guid.NewGuid(),
                   TenantId                  = tenantId,
                   EmployeeId                = employeeId,
                   AbsenceType               = type,
                   StartDate                 = start,
                   EndDate                   = end,
                   TotalWorkingDays          = days,
                   OfficialCertificateNumber = certificateNumber,
                   Notes                     = notes,
                   IsApproved                = false,
               };
    }

    /// <summary>
    /// Approves the absence request, formally granting the leave and updating payroll balance calculations.
    /// </summary>
    public void Approve() => IsApproved = true;

}
