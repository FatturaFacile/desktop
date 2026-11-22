// <copyright file="FieldServiceViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The Field Service Management module (GH-196, GH-197, GH-202): the hub that holds the dispatcher board, the maintenance
/// contracts, the work orders and the equipment history tabs, the sync of all of them and the outbox of the operations queued
/// offline, with the conflicts the server rejected.
/// </summary>
internal sealed partial class FieldServiceViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly FieldServiceLabels _labels;

    [ObservableProperty]
    private int _selectedTab;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _pendingText = string.Empty;

    [ObservableProperty]
    private bool _hasPending;

    [ObservableProperty]
    private bool _hasConflicts;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="board">The dispatcher board tab.</param>
    /// <param name="contracts">The maintenance contracts tab.</param>
    /// <param name="workOrders">The work orders tab.</param>
    /// <param name="equipment">The equipment history tab.</param>
    public FieldServiceViewModel(
        ISender sender,
        ILocalizationService localization,
        FieldServiceBoardViewModel board,
        MaintenanceContractsViewModel contracts,
        FieldWorkOrderViewModel workOrders,
        EquipmentHistoryViewModel equipment)
    {
        _sender = sender;
        _labels = new FieldServiceLabels(localization);
        Board = board;
        Contracts = contracts;
        WorkOrders = workOrders;
        Equipment = equipment;
        PendingText = _labels.Format("FieldService_Pending", "0");

        // A dispatch or a preventive run changed the work orders; a queued operation changed the outbox.
        board.DataChanged += (_, _) => _ = workOrders.RefreshAsync();
        workOrders.OutboxChanged += (_, _) => _ = RefreshOutboxAsync();
    }

    /// <summary>
    /// Gets the dispatcher board tab.
    /// </summary>
    public FieldServiceBoardViewModel Board { get; }

    /// <summary>
    /// Gets the maintenance contracts tab.
    /// </summary>
    public MaintenanceContractsViewModel Contracts { get; }

    /// <summary>
    /// Gets the work orders tab.
    /// </summary>
    public FieldWorkOrderViewModel WorkOrders { get; }

    /// <summary>
    /// Gets the equipment history tab.
    /// </summary>
    public EquipmentHistoryViewModel Equipment { get; }

    /// <summary>
    /// Gets the queued operations the server rejected and the user has not dismissed.
    /// </summary>
    public ObservableCollection<FieldServiceConflictRow> Conflicts { get; } = [];

    /// <summary>
    /// Loads every tab from the local cache (pulling what is missing) and the state of the outbox.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            await Board.LoadAsync();
            await Contracts.LoadAsync();
            await WorkOrders.LoadAsync();
            await Equipment.LoadAsync();
            await RefreshOutboxAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Sends the outbox first, so the server has the field operations, then pulls every tab.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public async Task SyncAllAsync()
    {
        IsBusy = true;

        try
        {
            await FlushCoreAsync();
            await Board.SyncAsync();
            await Contracts.SyncAsync();
            await WorkOrders.SyncAsync();
            await Equipment.SyncAsync();
            await RefreshOutboxAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Sends the queued field operations to the server, each with its idempotency key.
    /// </summary>
    /// <returns>A task representing the send.</returns>
    [RelayCommand]
    public async Task FlushAsync()
    {
        IsBusy = true;

        try
        {
            await FlushCoreAsync();
            await RefreshOutboxAsync();
            await WorkOrders.RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Dismisses a rejected operation.
    /// </summary>
    /// <param name="conflict">The conflict row.</param>
    /// <returns>A task representing the change.</returns>
    [RelayCommand]
    public async Task DismissConflictAsync(FieldServiceConflictRow? conflict)
    {
        if (conflict is null)
        {
            return;
        }

        _ = await _sender.Send(new DismissFieldServiceConflictCommand(conflict.Id));
        await RefreshOutboxAsync();
    }

    private async Task FlushCoreAsync()
    {
        var result = await _sender.Send(new FlushFieldServiceOutboxCommand());
        StatusMessage = result.Offline && result.Remaining > 0
            ? _labels.Format("FieldService_FlushOffline", result.Remaining.ToString(_labels.Culture))
            : _labels.Format("FieldService_FlushDone", result.Sent.ToString(_labels.Culture), result.Conflicts.ToString(_labels.Culture), result.Remaining.ToString(_labels.Culture));
    }

    private async Task RefreshOutboxAsync()
    {
        var summary = await _sender.Send(new GetFieldServiceOutboxQuery());
        PendingText = _labels.Format("FieldService_Pending", summary.Pending.ToString(_labels.Culture));
        HasPending = summary.Pending > 0;

        Conflicts.Clear();
        foreach (var conflict in summary.Conflicts)
        {
            Conflicts.Add(new FieldServiceConflictRow(
                conflict.Id,
                _labels.Format("FieldService_ConflictLine", _labels.Key("FieldService_Op_" + conflict.Operation), _labels.Moment(new DateTimeOffset(conflict.OccurredAtUtc)), conflict.Error)));
        }

        HasConflicts = Conflicts.Count > 0;
    }
}
