// <copyright file="AbsenceDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents absence dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="AbsenceType">The absence type.</param>
/// <param name="StartDate">The start date.</param>
/// <param name="EndDate">The end date.</param>
/// <param name="TotalWorkingDays">The total working days.</param>
/// <param name="IsApproved">The is approved.</param>
/// <param name="CertificateNumber">The certificate number.</param>
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
