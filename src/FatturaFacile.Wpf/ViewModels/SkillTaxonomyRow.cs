// <copyright file="SkillTaxonomyRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One skill of the taxonomy, ready to show.
/// </summary>
/// <param name="Skill">The skill.</param>
/// <param name="CertificationText">The certificate type the skill needs, or a dash.</param>
internal sealed record SkillTaxonomyRow(PlanningSkill Skill, string CertificationText);
