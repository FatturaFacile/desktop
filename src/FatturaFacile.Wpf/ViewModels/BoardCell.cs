// <copyright file="BoardCell.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One week of one employee on the allocation board: it shows the load and is where an allocation is dropped to move it.
/// </summary>
/// <param name="EmployeeId">The employee of the row.</param>
/// <param name="WeekStart">The Monday of the week.</param>
/// <param name="LoadText">The load of the week.</param>
/// <param name="BandName">The saturation band: None, Green, Yellow or Red.</param>
/// <param name="IsOverloaded">Whether the week is overloaded.</param>
/// <param name="ToolTip">The explanation shown on hover.</param>
internal sealed record BoardCell(Guid EmployeeId, DateOnly WeekStart, string LoadText, string BandName, bool IsOverloaded, string ToolTip);
