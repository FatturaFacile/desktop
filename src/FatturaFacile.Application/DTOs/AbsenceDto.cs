// <copyright file="AbsenceDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an employee leave or absence record.
/// </summary>
/// <param name="Id">The unique identifier of the absence record.</param>
/// <param name="TenantId">The tenant organization boundary owning this absence entry.</param>
/// <param name="EmployeeId">The unique identifier of the associated employee.</param>
/// <param name="AbsenceType">The categorization of the absence (e.g., vacation, illness, personal leave).</param>
/// <param name="StartDate">The inclusive starting date of the absence period.</param>
/// <param name="EndDate">The inclusive ending date of the absence period.</param>
/// <param name="TotalWorkingDays">The net calculated working days encompassed by the absence span.</param>
/// <param name="IsApproved">A flag indicating whether management or HR has formally approved this leave request.</param>
/// <param name="CertificateNumber">The optional medical or official justification certificate identifier.</param>
public record AbsenceDto(
    Guid Id,
    Guid TenantId,
    Guid EmployeeId,
    AbsenceType AbsenceType,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalWorkingDays,
    bool IsApproved,
    string? CertificateNumber);
