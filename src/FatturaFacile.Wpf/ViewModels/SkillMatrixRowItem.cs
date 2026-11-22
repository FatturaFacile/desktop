// <copyright file="SkillMatrixRowItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One employee of the skill matrix.
/// </summary>
/// <param name="EmployeeName">The employee.</param>
/// <param name="Cells">The skills.</param>
internal sealed record SkillMatrixRowItem(string EmployeeName, IReadOnlyList<SkillMatrixCellItem> Cells);
