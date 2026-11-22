// <copyright file="ReceivablesAgingCell.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One bucket figure of the aging; clicking it drills down to the items.
/// </summary>
/// <param name="Key">The bucket key.</param>
/// <param name="CustomerId">The customer, null for the company figure.</param>
/// <param name="Header">The bucket label.</param>
/// <param name="AmountText">The open amount.</param>
/// <param name="ShareText">The share of the total.</param>
internal sealed record ReceivablesAgingCell(string Key, Guid? CustomerId, string Header, string AmountText, string ShareText);
