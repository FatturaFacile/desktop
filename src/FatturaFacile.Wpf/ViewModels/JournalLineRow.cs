// <copyright file="JournalLineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An editable line of the Prima Nota entry: one account with a debit (Dare) or a credit (Avere), never both.
/// </summary>
internal sealed partial class JournalLineRow : ObservableObject
{
    [ObservableProperty]
    private ChartOfAccountDto? _account;

    [ObservableProperty]
    private decimal _debit;

    [ObservableProperty]
    private decimal _credit;

    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>
    /// Occurs when an amount or the account changes, so the entry totals can be recalculated.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Converts the line to the input of the create command.
    /// </summary>
    /// <returns>The line input.</returns>
    public JournalLineInput ToInput() =>
        new(Account?.Id ?? Guid.Empty, Debit, Credit, string.IsNullOrWhiteSpace(Description) ? null : Description.Trim());

    partial void OnDebitChanged(decimal value)
    {
        if (value != 0m && Credit != 0m)
        {
            Credit = 0m;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    partial void OnCreditChanged(decimal value)
    {
        if (value != 0m && Debit != 0m)
        {
            Debit = 0m;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    partial void OnAccountChanged(ChartOfAccountDto? value) => Changed?.Invoke(this, EventArgs.Empty);
}
