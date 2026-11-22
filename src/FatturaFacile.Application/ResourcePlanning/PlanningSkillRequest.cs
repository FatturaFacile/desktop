// <copyright file="PlanningSkillRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body that creates or updates a skill of the taxonomy.
/// </summary>
/// <param name="Category">The category.</param>
/// <param name="SkillName">The skill name.</param>
/// <param name="RequiresCertification">Whether a valid certificate is needed.</param>
/// <param name="CertificationCode">The certificate type, required when a certificate is needed.</param>
public sealed record PlanningSkillRequest(
    string Category,
    string SkillName,
    bool RequiresCertification,
    string? CertificationCode);
