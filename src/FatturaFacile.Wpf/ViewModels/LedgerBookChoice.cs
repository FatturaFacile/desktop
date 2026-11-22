// <copyright file="LedgerBookChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Accounting;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A ledger book of the selector and its localized label.
/// </summary>
/// <param name="Book">The ledger book.</param>
/// <param name="Label">The localized label.</param>
internal sealed record LedgerBookChoice(LedgerBook Book, string Label);
