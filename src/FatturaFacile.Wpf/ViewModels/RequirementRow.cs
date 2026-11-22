// <copyright file="RequirementRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A skill requirement of the candidate search.
/// </summary>
/// <param name="SkillId">The skill.</param>
/// <param name="SkillName">The skill name.</param>
/// <param name="MinLevel">The minimum proficiency.</param>
/// <param name="Text">The text of the row.</param>
internal sealed record RequirementRow(Guid SkillId, string SkillName, int MinLevel, string Text);
