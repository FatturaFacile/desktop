// <copyright file="KanbanCardRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One work package card of the Kanban board.
/// </summary>
/// <param name="Code">The WBS code.</param>
/// <param name="Title">The node title.</param>
/// <param name="PercentText">The formatted progress.</param>
/// <param name="DatesText">The planned dates.</param>
/// <param name="IsCritical">Whether the node is on the critical path.</param>
internal sealed record KanbanCardRow(string Code, string Title, string PercentText, string DatesText, bool IsCritical);
