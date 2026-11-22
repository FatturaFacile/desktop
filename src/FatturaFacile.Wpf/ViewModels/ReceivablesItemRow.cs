// <copyright file="ReceivablesItemRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A row of the open items list.
/// </summary>
/// <param name="Item">The item.</param>
/// <param name="Number">The document number.</param>
/// <param name="IssueText">The issue date.</param>
/// <param name="DueText">The due date.</param>
/// <param name="OriginalText">The original amount.</param>
/// <param name="OpenText">The open amount.</param>
/// <param name="StatusText">The status.</param>
/// <param name="State">The state used to colour the row: Open, Partial, Overdue, Disputed or Closed.</param>
/// <param name="DaysText">The days past due.</param>
internal sealed record ReceivablesItemRow(ReceivableItem Item, string Number, string IssueText, string DueText, string OriginalText, string OpenText, string StatusText, string State, string DaysText);
