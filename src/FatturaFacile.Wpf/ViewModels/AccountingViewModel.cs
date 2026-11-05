// <copyright file="AccountingViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Accounting.Queries;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model managing General Ledger (Prima Nota) and Chart of Accounts (Piano dei Conti).
/// </summary>
public sealed partial class AccountingViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<GeneralLedgerEntryDto> _entries = new();

    [ObservableProperty]
    private ObservableCollection<ChartOfAccountDto> _accounts = new();

    [ObservableProperty]
    private GeneralLedgerEntryDto? _selectedEntry;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountingViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    public AccountingViewModel(
        ISender sender,
        ICurrentTenantService currentTenantService,
        ILocalizationService localizationService)
    {
        _sender = sender;
        _currentTenantService = currentTenantService;
        _localizationService = localizationService;
        _statusMessage = localizationService["Common_Ready"];

        _currentTenantService.CurrentTenantChanged += (_, _) => _ = OnTenantChangedAsync();
    }

    /// <summary>
    /// Loads general ledger entries and chart of accounts.
    /// </summary>
    /// <returns>A task representing the load operation.</returns>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = _localizationService["Common_Loading"];
        try
        {
            var entries = await _sender.Send(new GetGeneralLedgerEntriesQuery());
            Entries.Clear();
            foreach (var entry in entries)
            {
                Entries.Add(entry);
            }

            SelectedEntry = Entries.FirstOrDefault();

            var accounts = await _sender.Send(new GetChartOfAccountsQuery());
            Accounts.Clear();
            foreach (var acc in accounts)
            {
                Accounts.Add(acc);
            }

            StatusMessage = $"{Entries.Count} {_localizationService["Accounting_GeneralLedger"]}, {Accounts.Count} {_localizationService["Accounting_ChartOfAccounts"]}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnTenantChangedAsync()
    {
        SelectedEntry = null;
        Entries.Clear();
        Accounts.Clear();
        await LoadDataAsync();
    }
}
