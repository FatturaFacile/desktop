// <copyright file="MatchedSkill.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A required skill that a candidate has.
/// </summary>
/// <param name="SkillId">The skill id.</param>
/// <param name="SkillName">The skill name.</param>
/// <param name="Level">The proficiency of the candidate.</param>
/// <param name="MinLevel">The proficiency that was required.</param>
/// <param name="ValidUntilDate">The valid until date.</param>
/// <param name="CertificationStatus">The certification status.</param>
public sealed record MatchedSkill(
    Guid SkillId,
    string SkillName,
    int Level,
    int MinLevel,
    DateOnly? ValidUntilDate,
    CertificationStatus CertificationStatus);
