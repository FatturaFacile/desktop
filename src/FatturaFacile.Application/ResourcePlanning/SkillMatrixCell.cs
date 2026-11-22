// <copyright file="SkillMatrixCell.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// One skill of one employee in the skill matrix.
/// </summary>
/// <param name="SkillId">The skill.</param>
/// <param name="SkillName">The skill name.</param>
/// <param name="Category">The category of the skill.</param>
/// <param name="Level">The proficiency from 1 to 5.</param>
/// <param name="State">Whether the skill counts for staffing.</param>
public sealed record SkillMatrixCell(Guid SkillId, string SkillName, string Category, int Level, SkillMatrixCellState State);
