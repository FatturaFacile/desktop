// <copyright file="CandidateBlocker.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A reason why a candidate does not qualify.
/// </summary>
/// <param name="Code">The code, for example skill_missing or insufficient_availability.</param>
/// <param name="SkillId">The skill concerned, or null.</param>
/// <param name="Params">The parameters of the message, or null.</param>
public sealed record CandidateBlocker(
    string Code,
    Guid? SkillId,
    JsonElement? Params);
