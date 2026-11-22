// <copyright file="SkillMatrixRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The skills of one employee in the skill matrix.
/// </summary>
/// <param name="EmployeeId">The employee.</param>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="Cells">The skills, by category and name.</param>
public sealed record SkillMatrixRow(Guid EmployeeId, string EmployeeName, IReadOnlyList<SkillMatrixCell> Cells);
