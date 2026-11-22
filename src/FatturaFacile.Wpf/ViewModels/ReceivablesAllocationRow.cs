// <copyright file="ReceivablesAllocationRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A row of the allocations of the selected item.
/// </summary>
/// <param name="Allocation">The allocation.</param>
/// <param name="TypeText">The kind of allocation.</param>
/// <param name="AmountText">The amount.</param>
/// <param name="DateText">The value date.</param>
/// <param name="ReferenceText">The reference or notes.</param>
/// <param name="StateText">Reversed, or empty.</param>
/// <param name="CanReverse">Whether the allocation can still be reversed.</param>
internal sealed record ReceivablesAllocationRow(ReceivableAllocation Allocation, string TypeText, string AmountText, string DateText, string ReferenceText, string StateText, bool CanReverse);
