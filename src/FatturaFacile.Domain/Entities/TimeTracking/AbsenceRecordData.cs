// <copyright file="AbsenceRecordData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.TimeTracking;

/// <summary>
/// Data transfer object holding leave interval, certificate, and employee details for a new <see cref="AbsenceRecord"/>.
/// </summary>
/// <param name="TenantId">The unique identifier of the employing tenant organization.</param>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
/// <param name="Type">The classification category of the absence.</param>
/// <param name="Start">The start date of the absence interval.</param>
/// <param name="End">The end date of the absence interval.</param>
/// <param name="Days">The calculated total working days encompassed by the leave.</param>
/// <param name="CertificateNumber">The optional official medical or statutory certificate identifier.</param>
/// <param name="Notes">Optional remarks or operational details regarding the absence.</param>
public sealed record AbsenceRecordData(
    Guid        TenantId,
    Guid        EmployeeId,
    AbsenceType Type,
    DateTime    Start,
    DateTime    End,
    decimal     Days,
    string?     CertificateNumber = null,
    string?     Notes             = null);
