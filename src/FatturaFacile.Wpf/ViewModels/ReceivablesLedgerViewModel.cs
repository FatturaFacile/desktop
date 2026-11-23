// <copyright file="ReceivablesLedgerViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The customer ledger with its running balance and the statement of account: preview, PDF and Excel export, and the email send.
/// </summary>
internal sealed partial class ReceivablesLedgerViewModel : ObservableObject
{
    private const int DefaultPeriodDays = 90;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;

    private Guid? _customerId;
    private string _customerLabel = string.Empty;
    private string? _currency;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private bool _hasCustomer;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _noticeText = string.Empty;

    [ObservableProperty]
    private string _closingText = string.Empty;

    [ObservableProperty]
    private bool _hasStatement;

    [ObservableProperty]
    private string _statementHeader = string.Empty;

    [ObservableProperty]
    private string _statementBalances = string.Empty;

    [ObservableProperty]
    private string _statementOpen = string.Empty;

    [ObservableProperty]
    private string _statementAging = string.Empty;

    [ObservableProperty]
    private string _recipient = string.Empty;

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _resultState = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesLedgerViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public ReceivablesLedgerViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        _sender = sender;
        _localization = localization;
        _time = time;
        _toDate = _time.GetLocalNow().Date;
        _fromDate = _toDate.AddDays(-DefaultPeriodDays);
    }

    /// <summary>
    /// Gets the movements of the ledger.
    /// </summary>
    public ObservableCollection<ReceivablesLedgerRow> Rows { get; } = [];

    /// <summary>
    /// Loads the ledger and the statement of a customer for the period on screen.
    /// </summary>
    /// <param name="customerId">The customer, null for none.</param>
    /// <param name="customerLabel">The customer name.</param>
    /// <param name="currency">The ISO currency, null for the company currency.</param>
    /// <returns>A task representing the load.</returns>
    public async Task LoadAsync(Guid? customerId, string customerLabel, string? currency)
    {
        _customerId = customerId;
        _customerLabel = customerLabel;
        _currency = currency;
        HasCustomer = customerId is not null;
        Rows.Clear();
        HasStatement = false;

        if (customerId is not { } id)
        {
            ClosingText = string.Empty;
            NoticeText = string.Empty;
            StatusMessage = _localization["Receivables_SelectCustomer"];

            return;
        }

        IsBusy = true;

        try
        {
            var from = DateOnly.FromDateTime(FromDate);
            var to = DateOnly.FromDateTime(ToDate);
            var ledger = await _sender.Send(new GetCustomerLedgerQuery(id, from, to, currency));
            ApplyLedger(ledger);
            var statement = await _sender.Send(new GetCustomerStatementQuery(id, from, to, currency));
            ApplyStatement(statement);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync(_customerId, _customerLabel, _currency);

    [RelayCommand]
    private Task ExportPdfAsync() => ExportAsync("pdf");

    [RelayCommand]
    private Task ExportExcelAsync() => ExportAsync("xlsx");

    [RelayCommand]
    private async Task SendStatementAsync()
    {
        if (_customerId is not { } id)
        {
            return;
        }

        try
        {
            SendStatementRequest request = new(
                Guid.NewGuid(),
                id,
                _customerLabel,
                DateOnly.FromDateTime(FromDate),
                DateOnly.FromDateTime(ToDate),
                string.IsNullOrWhiteSpace(Recipient) ? null : Recipient.Trim(),
                LocaleOf(_localization.CurrentCulture),
                _time.GetUtcNow().UtcDateTime);
            var result = await _sender.Send(new SendCustomerStatementCommand(request));
            var (text, state) = ReceivablesFormat.Describe(result, _localization);
            Show(text, state);
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, ReceivablesFormat.Bad);
        }
    }

    private static string LocaleOf(CultureInfo culture) => culture.TwoLetterISOLanguageName switch
    {
        "de" => "de",
        "fr" => "fr",
        "it" => "it",
        _ => "en",
    };

    private async Task ExportAsync(string format)
    {
        if (_customerId is not { } id)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _sender.Send(new ExportCustomerStatementCommand(id, format, DateOnly.FromDateTime(FromDate), DateOnly.FromDateTime(ToDate), _currency));
            if (result.Offline || result.Path is null)
            {
                Show(_localization["Receivables_ExportOffline"], ReceivablesFormat.Warn);
            }
            else
            {
                Show(string.Format(_localization.CurrentCulture, _localization["Receivables_Exported"], result.Path), ReceivablesFormat.Good);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, ReceivablesFormat.Bad);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Show(string text, string state)
    {
        ResultText = text;
        ResultState = state;
    }

    private void ApplyLedger(ReceivablesLoad<CustomerLedger>? load)
    {
        if (load is null)
        {
            StatusMessage = _localization["Receivables_NoData"];
            NoticeText = string.Empty;
            ClosingText = string.Empty;

            return;
        }

        var culture = _localization.CurrentCulture;
        NoticeText = load.FromCache ? _localization["Receivables_OfflineCache"] : string.Empty;
        StatusMessage = load.Data.Movements.Count == 0 ? _localization["Receivables_NoMovements"] : string.Empty;
        ClosingText = string.Format(culture, _localization["Receivables_ClosingBalance"], ReceivablesFormat.Money(load.Data.ClosingBalance, culture));

        foreach (var movement in load.Data.Movements)
        {
            Rows.Add(new ReceivablesLedgerRow(
                ReceivablesFormat.Day(movement.Date, culture),
                movement.Kind,
                movement.Reference.Length > 0 ? movement.Reference : movement.DocumentType,
                movement.Debit > 0m ? ReceivablesFormat.Money(movement.Debit, culture) : string.Empty,
                movement.Credit > 0m ? ReceivablesFormat.Money(movement.Credit, culture) : string.Empty,
                ReceivablesFormat.Money(movement.Balance, culture),
                movement.Status ?? string.Empty));
        }
    }

    private void ApplyStatement(ReceivablesLoad<CustomerStatement>? load)
    {
        HasStatement = load is not null;

        if (load is null)
        {
            return;
        }

        var culture = _localization.CurrentCulture;
        var statement = load.Data;
        StatementHeader = string.Format(culture, _localization["Receivables_StatementHeader"], statement.CustomerName, ReceivablesFormat.Day(statement.From, culture), ReceivablesFormat.Day(statement.To, culture));
        StatementBalances = string.Format(culture, _localization["Receivables_StatementBalances"], ReceivablesFormat.Money(statement.OpeningBalance, culture), ReceivablesFormat.Money(statement.ClosingBalance, culture), statement.Currency);
        StatementOpen = string.Format(culture, _localization["Receivables_StatementOpen"], statement.OpenItems.Count.ToString(culture), ReceivablesFormat.Money(statement.TotalOpen, culture), ReceivablesFormat.Money(statement.TotalOverdue, culture));
        StatementAging = string.Join("; ", statement.Aging.Where(pair => pair.Value != 0m).Select(pair => pair.Key + ": " + ReceivablesFormat.Money(pair.Value, culture)));
    }
}
