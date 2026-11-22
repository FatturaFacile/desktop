// <copyright file="EmployeeSkillProfileRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body that sets the proficiency of an employee in a skill (upsert on employee and skill).
/// </summary>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="SkillId">The skill id.</param>
/// <param name="ProficiencyLevel">The proficiency from 1 to 5.</param>
/// <param name="ValidUntilDate">The last day the proficiency is valid, or null.</param>
public sealed record EmployeeSkillProfileRequest(
    Guid EmployeeId,
    Guid SkillId,
    int ProficiencyLevel,
    DateOnly? ValidUntilDate);
