// <copyright file="BoardRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One employee of the allocation board with the weeks an allocation can be dropped on.
/// </summary>
/// <param name="EmployeeName">The employee.</param>
/// <param name="Cells">The weeks.</param>
internal sealed record BoardRow(string EmployeeName, IReadOnlyList<BoardCell> Cells);
