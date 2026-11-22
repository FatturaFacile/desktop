// <copyright file="LedgerReportsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Accounting.Queries;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Inspection of the posted ledger: the trial balance (two or four columns) per ledger book and period, and the account ledger
/// (mastrino) with running balance of one account, reachable by drill-down from a trial balance row.
/// </summary>
internal sealed partial class LedgerReportsViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    [ObservableProperty]
    private DateTime _from = new(DateTime.Today.Year, 1, 1);

    [ObservableProperty]
    private DateTime _to = DateTime.Today;

    [ObservableProperty]
    private LedgerBookChoice _selectedLedger;

    [ObservableProperty]
    private bool _twoColumns;

    [ObservableProperty]
    private decimal _totalDebit;

    [ObservableProperty]
    private decimal _totalCredit;

    [ObservableProperty]
    private decimal _totalDebitBalance;

    [ObservableProperty]
    private decimal _totalCreditBalance;

    [ObservableProperty]
    private string _balanceMessage = string.Empty;

    [ObservableProperty]
    private TrialBalanceRow? _selectedRow;

    [ObservableProperty]
    private ChartOfAccountDto? _selectedAccount;

    [ObservableProperty]
    private decimal _openingBalance;

    [ObservableProperty]
    private decimal _closingBalance;

    [ObservableProperty]
    private string _statusMessage;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Initializes a new instance of the <see cref="LedgerReportsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="localization">The localization service.</param>
    public LedgerReportsViewModel(ISender sender, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(localization);

        _sender = sender;
        _localization = localization;
        LedgerBooks = LedgerBookChoices.Build(localization);
        _selectedLedger = LedgerBooks[0];
        _statusMessage = localization["Common_Ready"];
    }

    /// <summary>
    /// Gets the ledger books of the selector.
    /// </summary>
    public IReadOnlyList<LedgerBookChoice> LedgerBooks { get; }

    /// <summary>
    /// Gets the accounts of the chart.
    /// </summary>
    public ObservableCollection<ChartOfAccountDto> Accounts { get; } = [];

    /// <summary>
    /// Gets the rows of the trial balance.
    /// </summary>
    public ObservableCollection<TrialBalanceRow> TrialBalanceRows { get; } = [];

    /// <summary>
    /// Gets the lines of the account ledger.
    /// </summary>
    public ObservableCollection<AccountLedgerLine> AccountLedgerLines { get; } = [];

    /// <summary>
    /// Loads the chart of accounts and the trial balance of the current period.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var accounts = await _sender.Send(new GetChartOfAccountsQuery());
        Accounts.Clear();
        foreach (var account in accounts)
        {
            Accounts.Add(account);
        }

        await CalculateTrialBalanceCoreAsync();
    });

    [RelayCommand]
    private Task CalculateTrialBalanceAsync() => RunAsync(CalculateTrialBalanceCoreAsync);

    [RelayCommand]
    private Task CalculateAccountLedgerAsync() => RunAsync(CalculateAccountLedgerCoreAsync);

    partial void OnSelectedRowChanged(TrialBalanceRow? value)
    {
        if (value is null)
        {
            return;
        }

        SelectedAccount = Accounts.FirstOrDefault(a => a.Id == value.AccountId);
    }

    partial void OnSelectedAccountChanged(ChartOfAccountDto? value)
    {
        if (value is not null)
        {
            _ = RunAsync(CalculateAccountLedgerCoreAsync);
        }
    }

    private async Task CalculateTrialBalanceCoreAsync()
    {
        var result = await _sender.Send(new GetTrialBalanceQuery(From, To, SelectedLedger.Book, TwoColumns ? 2 : 4));

        TrialBalanceRows.Clear();
        foreach (var row in result.Rows)
        {
            TrialBalanceRows.Add(row);
        }

        TotalDebit = result.TotalDebit;
        TotalCredit = result.TotalCredit;
        TotalDebitBalance = result.TotalDebitBalance;
        TotalCreditBalance = result.TotalCreditBalance;
        BalanceMessage = _localization[result.IsBalanced ? "Gl_TrialBalanceBalanced" : "Gl_TrialBalanceUnbalanced"];
        StatusMessage = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", SelectedLedger.Label, BalanceMessage);
    }

    private async Task CalculateAccountLedgerCoreAsync()
    {
        if (SelectedAccount is not { } account)
        {
            return;
        }

        var result = await _sender.Send(new GetAccountLedgerQuery(account.Id, From, To, SelectedLedger.Book, PerPage: 500));

        AccountLedgerLines.Clear();
        if (result is null)
        {
            OpeningBalance = 0m;
            ClosingBalance = 0m;
            return;
        }

        foreach (var line in result.Lines)
        {
            AccountLedgerLines.Add(line);
        }

        OpeningBalance = result.OpeningBalance;
        ClosingBalance = result.ClosingBalance;
    }

    private async Task RunAsync(Func<Task> operation)
    {
        IsBusy = true;
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            StatusMessage = JournalMessages.Describe(ex, _localization);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
