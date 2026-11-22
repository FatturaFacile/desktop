// <copyright file="ReceivablesLedgerRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A row of the customer ledger.
/// </summary>
/// <param name="DateText">The date.</param>
/// <param name="KindText">The kind of movement.</param>
/// <param name="Reference">The document reference.</param>
/// <param name="DebitText">The debit.</param>
/// <param name="CreditText">The credit.</param>
/// <param name="BalanceText">The running balance.</param>
/// <param name="StatusText">The status.</param>
internal sealed record ReceivablesLedgerRow(string DateText, string KindText, string Reference, string DebitText, string CreditText, string BalanceText, string StatusText);
