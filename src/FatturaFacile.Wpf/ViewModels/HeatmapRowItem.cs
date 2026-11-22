// <copyright file="HeatmapRowItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One employee row of the capacity heatmap.
/// </summary>
/// <param name="EmployeeName">The employee.</param>
/// <param name="DepartmentName">The department, or a dash.</param>
/// <param name="Cells">The cells in the order of the period headers.</param>
/// <param name="HasOverload">Whether any cell is overloaded.</param>
internal sealed record HeatmapRowItem(string EmployeeName, string DepartmentName, IReadOnlyList<HeatmapCellItem> Cells, bool HasOverload);
