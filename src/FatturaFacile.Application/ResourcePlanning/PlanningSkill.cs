// <copyright file="PlanningSkill.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A skill of the skill taxonomy (category and name), optionally tied to a certificate that must be valid.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="Category">The category.</param>
/// <param name="SkillName">The skill name.</param>
/// <param name="RequiresCertification">Whether a valid certificate is needed for the skill to count.</param>
/// <param name="CertificationCode">The Mod 099 certificate type that must be valid, or null.</param>
public sealed record PlanningSkill(
    Guid Id,
    string Category,
    string SkillName,
    bool RequiresCertification,
    string? CertificationCode);
