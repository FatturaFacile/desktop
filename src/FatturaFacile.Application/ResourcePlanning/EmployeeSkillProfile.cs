// <copyright file="EmployeeSkillProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The proficiency of one employee in one skill with the state of the certificate the skill may require.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="SkillId">The skill id.</param>
/// <param name="SkillName">The skill name.</param>
/// <param name="Category">The category.</param>
/// <param name="ProficiencyLevel">The proficiency from 1 to 5.</param>
/// <param name="ValidUntilDate">The last day the proficiency is valid, or null.</param>
/// <param name="IsValid">Whether the proficiency is valid today.</param>
/// <param name="RequiresCertification">Whether the skill needs a certificate.</param>
/// <param name="CertificationCode">The certificate type, or null.</param>
/// <param name="CertificationStatus">The certification status.</param>
/// <param name="CertificationValidUntil">The expiry of the certificate, or null.</param>
public sealed record EmployeeSkillProfile(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid SkillId,
    string SkillName,
    string Category,
    int ProficiencyLevel,
    DateOnly? ValidUntilDate,
    bool IsValid,
    bool RequiresCertification,
    string? CertificationCode,
    CertificationStatus CertificationStatus,
    DateOnly? CertificationValidUntil);
