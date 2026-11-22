// <copyright file="KanbanColumnRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One column of the Kanban board (not started, in progress, done).
/// </summary>
/// <param name="Title">The localized column title with the card count.</param>
/// <param name="Cards">The cards of the column.</param>
internal sealed record KanbanColumnRow(string Title, IReadOnlyList<KanbanCardRow> Cards);
