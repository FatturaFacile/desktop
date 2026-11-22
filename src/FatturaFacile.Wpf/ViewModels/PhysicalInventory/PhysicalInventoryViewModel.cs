// <copyright file="PhysicalInventoryViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Back-office module of the physical inventory (Mod 058): ABC analysis, cycle count plans, count sessions and the
/// variance reconciliation with its two approval levels. It hosts one view model per tab.
/// </summary>
internal sealed partial class PhysicalInventoryViewModel : ObservableObject
{
    /// <summary>
    /// The index of the reconciliation tab.
    /// </summary>
    public const int ReconciliationTab = 3;

    [ObservableProperty]
    private int _selectedTabIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalInventoryViewModel"/> class.
    /// </summary>
    /// <param name="client">The physical inventory web-api client.</param>
    /// <param name="localization">The localization service.</param>
    public PhysicalInventoryViewModel(IInventoryCountingApiClient client, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(localization);

        Abc = new AbcAnalysisViewModel(client, localization);
        Plans = new CyclePlansViewModel(client, localization);
        Sessions = new CountSessionsViewModel(client, localization);
        Reconciliation = new ReconciliationViewModel(client, localization);
        Sessions.ReconciliationRequested += (_, session) => _ = ShowReconciliationAsync(session);
    }

    /// <summary>
    /// Gets the ABC analysis tab.
    /// </summary>
    public AbcAnalysisViewModel Abc { get; }

    /// <summary>
    /// Gets the cycle count plans tab.
    /// </summary>
    public CyclePlansViewModel Plans { get; }

    /// <summary>
    /// Gets the count sessions tab.
    /// </summary>
    public CountSessionsViewModel Sessions { get; }

    /// <summary>
    /// Gets the reconciliation tab.
    /// </summary>
    public ReconciliationViewModel Reconciliation { get; }

    /// <summary>
    /// Loads the data of the ABC, plans and sessions tabs; the reconciliation loads when a session is opened.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await Abc.LoadAsync();
        await Plans.LoadAsync();
        await Sessions.LoadAsync();
    }

    /// <summary>
    /// Shows the reconciliation of a session in the reconciliation tab.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>A task representing the load.</returns>
    public Task ShowReconciliationAsync(CountSessionRow session)
    {
        ArgumentNullException.ThrowIfNull(session);

        SelectedTabIndex = ReconciliationTab;
        return Reconciliation.OpenAsync(session.Id, session.SessionNumber);
    }
}
