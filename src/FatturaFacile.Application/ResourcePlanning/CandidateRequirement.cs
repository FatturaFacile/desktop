// <copyright file="CandidateRequirement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A skill with the minimum proficiency a candidate needs.
/// </summary>
/// <param name="SkillId">The skill id.</param>
/// <param name="MinLevel">The minimum proficiency from 1 to 5.</param>
public sealed record CandidateRequirement(
    Guid SkillId,
    int MinLevel);
