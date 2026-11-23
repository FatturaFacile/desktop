// <copyright file="ReceivablesAgingRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The aging of one customer.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="Name">The customer name.</param>
/// <param name="TotalText">The total open amount.</param>
/// <param name="OverdueText">The overdue amount.</param>
/// <param name="Cells">The bucket figures.</param>
internal sealed record ReceivablesAgingRow(Guid CustomerId, string Name, string TotalText, string OverdueText, IReadOnlyList<ReceivablesAgingCell> Cells);
