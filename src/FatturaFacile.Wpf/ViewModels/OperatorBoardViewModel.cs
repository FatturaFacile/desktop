// <copyright file="OperatorBoardViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Mes;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the MES operator board: the phases of a work centre with live man / machine / setup / run / downtime counters, the
/// actions the state machine allows, the suspension, declaration and lot panels and the offline state. Every action goes through
/// the offline-first service, so a disconnected station keeps working: commands queue with the instant the operator acted, the
/// board shows their effect and the outbox is sent again by the timer and when the network comes back.
/// </summary>
internal sealed partial class OperatorBoardViewModel : ObservableObject
{
    private const int FlushIntervalTicks = 15;
    private const string ZeroQuantity = "0";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ICurrentUserService _user;
    private readonly TimeProvider _time;
    private readonly INetworkStatus _network;
    private readonly TaskScheduler _uiScheduler = SynchronizationContext.Current is null ? TaskScheduler.Default : TaskScheduler.FromCurrentSynchronizationContext();

    private int _ticksSinceFlush;
    private bool _flushing;
    private bool _suppressReload;

    [ObservableProperty]
    private MesWorkCenter? _selectedWorkCenter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSuspendPanelOpen))]
    [NotifyPropertyChangedFor(nameof(IsDeclarePanelOpen))]
    [NotifyPropertyChangedFor(nameof(IsLotPanelOpen))]
    [NotifyPropertyChangedFor(nameof(IsClosePanelOpen))]
    [NotifyPropertyChangedFor(nameof(IsAnyPanelOpen))]
    private MesPanel _activePanel;

    [ObservableProperty]
    private MesPhaseCard? _activeCard;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _declarationGood = ZeroQuantity;

    [ObservableProperty]
    private string _declarationScrap = ZeroQuantity;

    [ObservableProperty]
    private bool _isScrapActive;

    [ObservableProperty]
    private MesChoice<MesDefectReason>? _selectedDefectReason;

    [ObservableProperty]
    private MesComponent? _selectedComponent;

    [ObservableProperty]
    private string _lotCode = string.Empty;

    [ObservableProperty]
    private string _lotQuantity = ZeroQuantity;

    [ObservableProperty]
    private bool _overConsumptionConfirmed;

    [ObservableProperty]
    private bool _needsOverConsumptionConfirm;

    [ObservableProperty]
    private IReadOnlyList<MesChoice<MesSuspensionReason>> _suspensionReasons = [];

    [ObservableProperty]
    private IReadOnlyList<MesChoice<MesDefectReason>> _defectReasons = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="OperatorBoardViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="user">The signed-in user.</param>
    /// <param name="time">The clock.</param>
    /// <param name="network">The network status.</param>
    public OperatorBoardViewModel(ISender sender, ILocalizationService localization, ICurrentUserService user, TimeProvider time, INetworkStatus network)
    {
        _sender = sender;
        _localization = localization;
        _user = user;
        _time = time;
        _network = network;
        LastSyncedText = _localization["Mes_NeverSynced"];
        BuildChoices();
        _localization.CultureChanged += (_, _) => BuildChoices();
        _network.Changed += OnNetworkChanged;
    }

    /// <summary>
    /// Gets the work centres the operator can pick.
    /// </summary>
    public ObservableCollection<MesWorkCenter> WorkCenters { get; } = [];

    /// <summary>
    /// Gets the phases of the selected work centre.
    /// </summary>
    public ObservableCollection<MesPhaseCard> Phases { get; } = [];

    /// <summary>
    /// Gets the commands the server refused and the operator has not dismissed.
    /// </summary>
    public ObservableCollection<MesConflictItem> Conflicts { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the suspension panel is open.
    /// </summary>
    public bool IsSuspendPanelOpen => ActivePanel == MesPanel.Suspend;

    /// <summary>
    /// Gets a value indicating whether the declaration panel is open.
    /// </summary>
    public bool IsDeclarePanelOpen => ActivePanel == MesPanel.Declare;

    /// <summary>
    /// Gets a value indicating whether the lot panel is open.
    /// </summary>
    public bool IsLotPanelOpen => ActivePanel == MesPanel.Lot;

    /// <summary>
    /// Gets a value indicating whether the close confirmation is open.
    /// </summary>
    public bool IsClosePanelOpen => ActivePanel == MesPanel.Close;

    /// <summary>
    /// Gets a value indicating whether any panel is open.
    /// </summary>
    public bool IsAnyPanelOpen => ActivePanel != MesPanel.None;

    /// <summary>
    /// Gets a value indicating whether there are refused commands to show.
    /// </summary>
    public bool HasConflicts => Conflicts.Count > 0;

    /// <summary>
    /// Gets the components of the phase the lot panel is open on.
    /// </summary>
    public IReadOnlyList<MesComponent> Components => ActiveCard?.Row.Components ?? [];

    /// <summary>
    /// Loads the work centres and the board of the selected one.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var centers = await _sender.Send(new GetMesWorkCentersQuery());
            WorkCenters.Clear();
            foreach (var center in centers)
            {
                WorkCenters.Add(center);
            }

            _suppressReload = true;
            SelectedWorkCenter = WorkCenters.FirstOrDefault(center => center.Id == SelectedWorkCenter?.Id) ?? WorkCenters.FirstOrDefault();
            _suppressReload = false;

            await ReloadBoardAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, isError: true);
        }
        catch (HttpRequestException ex)
        {
            Show(Format("Mes_Err_Server", ex.Message), isError: true);
        }
        catch (JsonException ex)
        {
            Show(Format("Mes_Err_Server", ex.Message), isError: true);
        }
        finally
        {
            _suppressReload = false;
            IsBusy = false;
        }
    }

    /// <summary>
    /// Reads the board again (sending the outbox first).
    /// </summary>
    /// <returns>A task representing the refresh.</returns>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;

        try
        {
            await ReloadBoardAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Advances the live counters; the view calls it every second. It also sends the outbox from time to time while commands wait.
    /// </summary>
    public void Tick()
    {
        var now = Now();
        foreach (var card in Phases)
        {
            card.Tick(now);
        }

        _ticksSinceFlush++;
        if (PendingCount > 0 && !_flushing && _ticksSinceFlush >= FlushIntervalTicks)
        {
            _ticksSinceFlush = 0;
            _ = FlushQuietlyAsync();
        }
    }

    private static string FormatTimestamp(DateTime utc, CultureInfo culture) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("g", culture);

    partial void OnSelectedWorkCenterChanged(MesWorkCenter? value)
    {
        if (value is not null && !_suppressReload)
        {
            _ = RefreshAsync();
        }
    }

    partial void OnActiveCardChanged(MesPhaseCard? value) => OnPropertyChanged(nameof(Components));

    [RelayCommand]
    private Task TakeOverAsync(MesPhaseCard? card)
    {
        if (card is null || _user.User is not { } signedIn)
        {
            Show(_localization["Mes_Err_NotSignedIn"], isError: true);
            return Task.CompletedTask;
        }

        return ExecuteAsync(MesCommand.TakeOver(card.OperationId, card.Row.PhaseId, signedIn.UserId, Now()), card);
    }

    [RelayCommand]
    private Task StartSetupAsync(MesPhaseCard? card) =>
        card is null ? Task.CompletedTask : ExecuteAsync(MesCommand.StartSetup(card.Row, Now()), card);

    [RelayCommand]
    private Task StartAttendedAsync(MesPhaseCard? card) =>
        card is null ? Task.CompletedTask : ExecuteAsync(MesCommand.StartProduction(card.Row, MesMachineMode.Attended, Now()), card);

    [RelayCommand]
    private Task StartUnattendedAsync(MesPhaseCard? card) =>
        card is null ? Task.CompletedTask : ExecuteAsync(MesCommand.StartProduction(card.Row, MesMachineMode.Unattended, Now()), card);

    [RelayCommand]
    private Task ResumeAsync(MesPhaseCard? card) =>
        card is null ? Task.CompletedTask : ExecuteAsync(MesCommand.Resume(card.Row, Now()), card);

    [RelayCommand]
    private void OpenSuspend(MesPhaseCard? card) => OpenPanel(MesPanel.Suspend, card);

    [RelayCommand]
    private void OpenDeclaration(MesPhaseCard? card)
    {
        DeclarationGood = ZeroQuantity;
        DeclarationScrap = ZeroQuantity;
        IsScrapActive = false;
        SelectedDefectReason = null;
        OpenPanel(MesPanel.Declare, card);
    }

    [RelayCommand]
    private void OpenLot(MesPhaseCard? card)
    {
        LotCode = string.Empty;
        LotQuantity = ZeroQuantity;
        OverConsumptionConfirmed = false;
        NeedsOverConsumptionConfirm = false;
        OpenPanel(MesPanel.Lot, card);
        SelectedComponent = card?.Row.Components.FirstOrDefault(component => component.LotTracked) ?? card?.Row.Components.FirstOrDefault();
    }

    [RelayCommand]
    private void OpenClose(MesPhaseCard? card) => OpenPanel(MesPanel.Close, card);

    [RelayCommand]
    private void ClosePanel()
    {
        ActivePanel = MesPanel.None;
        ActiveCard = null;
    }

    [RelayCommand]
    private async Task ConfirmSuspendAsync(MesChoice<MesSuspensionReason>? reason)
    {
        if (ActiveCard is not { } card || reason is null)
        {
            return;
        }

        if (await ExecuteAsync(MesCommand.Suspend(card.Row, reason.Value, Now()), card))
        {
            ClosePanel();
        }
    }

    [RelayCommand]
    private async Task ConfirmDeclarationAsync()
    {
        if (ActiveCard is not { } card)
        {
            return;
        }

        var defect = SelectedDefectReason?.Value;
        var command = MesCommand.Declare(card.Row, MesKeypad.Parse(DeclarationGood), MesKeypad.Parse(DeclarationScrap), defect, Now());
        if (await ExecuteAsync(command, card))
        {
            ClosePanel();
        }
    }

    [RelayCommand]
    private async Task ConfirmLotAsync()
    {
        if (ActiveCard is not { } card || SelectedComponent is not { } component)
        {
            return;
        }

        var quantity = MesKeypad.Parse(LotQuantity);
        var exceeds = component.RequiredQuantity > 0 && component.ConsumedQuantity + quantity > component.RequiredQuantity;
        if (exceeds && !OverConsumptionConfirmed)
        {
            NeedsOverConsumptionConfirm = true;
            Show(_localization["Mes_Err_OverConsumption"], isError: true);
            return;
        }

        var lot = LotCode.Trim().ToUpperInvariant();
        var command = MesCommand.ConsumeMaterial(card.Row, component.ItemId, lot, quantity, OverConsumptionConfirmed, Now());
        if (await ExecuteAsync(command, card))
        {
            ClosePanel();
        }
    }

    [RelayCommand]
    private async Task ConfirmCloseAsync()
    {
        if (ActiveCard is not { } card)
        {
            return;
        }

        if (await ExecuteAsync(MesCommand.Close(card.Row, Now()), card))
        {
            ClosePanel();
        }
    }

    [RelayCommand]
    private async Task DismissConflictAsync(MesConflictItem? conflict)
    {
        if (conflict is null)
        {
            return;
        }

        try
        {
            await _sender.Send(new DismissMesConflictCommand(conflict.CommandId));
            await ReloadBoardAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, isError: true);
        }
    }

    [RelayCommand]
    private async Task FlushAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _sender.Send(new FlushMesOutboxCommand());
            Show(
                result.Offline ? _localization["Mes_Offline"] : Format("Mes_FlushResult", result.Sent.ToString(CultureInfo.CurrentCulture)),
                isError: false);
            await ReloadBoardAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Keypad(string? key)
    {
        if (key is null)
        {
            return;
        }

        if (IsLotPanelOpen)
        {
            LotQuantity = MesKeypad.Apply(LotQuantity, key);
        }
        else if (IsScrapActive)
        {
            DeclarationScrap = MesKeypad.Apply(DeclarationScrap, key);
        }
        else
        {
            DeclarationGood = MesKeypad.Apply(DeclarationGood, key);
        }
    }

    [RelayCommand]
    private void AddPieces(string? pieces)
    {
        if (!int.TryParse(pieces, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count))
        {
            return;
        }

        if (IsLotPanelOpen)
        {
            LotQuantity = MesKeypad.Add(LotQuantity, count);
        }
        else if (IsScrapActive)
        {
            DeclarationScrap = MesKeypad.Add(DeclarationScrap, count);
        }
        else
        {
            DeclarationGood = MesKeypad.Add(DeclarationGood, count);
        }
    }

    [RelayCommand]
    private void SelectGoodField() => IsScrapActive = false;

    [RelayCommand]
    private void SelectScrapField() => IsScrapActive = true;

    [RelayCommand]
    private void ChooseDefectReason(MesChoice<MesDefectReason>? reason) => SelectedDefectReason = reason;

    [RelayCommand]
    private void ChooseComponent(MesComponent? component)
    {
        SelectedComponent = component;
        NeedsOverConsumptionConfirm = false;
        OverConsumptionConfirmed = false;
    }

    private DateTime Now() => _time.GetUtcNow().UtcDateTime;

    private string Format(string key, string argument) => string.Format(_localization.CurrentCulture, _localization[key], argument);

    private void Show(string message, bool isError)
    {
        StatusMessage = message;
        IsError = isError;
    }

    private void OpenPanel(MesPanel panel, MesPhaseCard? card)
    {
        if (card is null)
        {
            return;
        }

        ActiveCard = card;
        ActivePanel = panel;
    }

    private void BuildChoices()
    {
        SuspensionReasons = [.. Enum.GetValues<MesSuspensionReason>().Select(reason => new MesChoice<MesSuspensionReason>(reason, _localization["Mes_Reason_" + reason]))];
        DefectReasons = [.. Enum.GetValues<MesDefectReason>().Select(reason => new MesChoice<MesDefectReason>(reason, _localization["Mes_Defect_" + reason]))];
    }

    private void OnNetworkChanged(object? sender, EventArgs e)
    {
        if (_network.IsOnline)
        {
            _ = Task.Factory.StartNew(FlushQuietlyAsync, CancellationToken.None, TaskCreationOptions.DenyChildAttach, _uiScheduler).Unwrap();
        }
    }

    private async Task<bool> ExecuteAsync(MesCommand command, MesPhaseCard card)
    {
        IsBusy = true;

        try
        {
            var result = await _sender.Send(new ExecuteMesCommandCommand(command, card.Row));
            ShowResult(result);
            await ReloadBoardAsync();

            return result.Status != MesExecuteStatus.Rejected;
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, isError: true);

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowResult(MesExecuteResult result)
    {
        var (message, isError) = result.Status switch
        {
            MesExecuteStatus.Rejected => (_localization[result.ErrorKey ?? MesOperatorService.NotAllowedKey], true),
            MesExecuteStatus.Conflict => (Format("Mes_Err_Conflict", result.Detail ?? string.Empty), true),
            MesExecuteStatus.Queued => (_localization["Mes_QueuedOffline"], false),
            MesExecuteStatus.Sent => (string.Empty, false),
            _ => (string.Empty, false),
        };
        Show(message, isError);
    }

    private async Task FlushQuietlyAsync()
    {
        if (_flushing)
        {
            return;
        }

        _flushing = true;

        try
        {
            _ = await _sender.Send(new FlushMesOutboxCommand());
            await ReloadBoardAsync();
        }
        catch (UnauthorizedAccessException)
        {
            // Signed out: the commands wait in the outbox for the next sign-in.
        }
        catch (HttpRequestException)
        {
            // Still offline: the next tick or network change tries again.
        }
        finally
        {
            _flushing = false;
        }
    }

    private async Task ReloadBoardAsync()
    {
        if (SelectedWorkCenter is not { } center)
        {
            return;
        }

        Apply(await _sender.Send(new GetMesBoardQuery(center.Id)));
    }

    private void Apply(MesBoardView? view)
    {
        if (view is null)
        {
            Phases.Clear();
            Conflicts.Clear();
            IsOffline = true;
            OnPropertyChanged(nameof(HasConflicts));
            Show(_localization["Mes_NoBoardCached"], isError: true);

            return;
        }

        var now = Now();
        var culture = _localization.CurrentCulture;
        IsOffline = view.FromCache;
        PendingCount = view.PendingCount;
        LastSyncedText = view.SyncedAtUtc is { } synced
            ? Format("Mes_LastSynced", FormatTimestamp(synced, culture))
            : _localization["Mes_NeverSynced"];

        SyncPhases(view.Rows, now);
        SyncConflicts(view.Conflicts, culture);

        if (ActiveCard is { } active && !Phases.Contains(active))
        {
            ClosePanel();
        }
    }

    private void SyncPhases(IReadOnlyList<MesPhaseRow> rows, DateTime now)
    {
        var existing = Phases.ToDictionary(card => card.OperationId);
        var wanted = rows.Select(row => row.OperationId).ToHashSet();

        foreach (var stale in Phases.Where(card => !wanted.Contains(card.OperationId)).ToList())
        {
            _ = Phases.Remove(stale);
        }

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            MesPhaseCard card;
            if (existing.TryGetValue(row.OperationId, out var known))
            {
                known.Update(row, now);
                card = known;
            }
            else
            {
                card = new MesPhaseCard(row, now, _localization);
            }

            Place(card, index);
        }
    }

    private void Place(MesPhaseCard card, int index)
    {
        var current = Phases.IndexOf(card);
        if (current < 0)
        {
            Phases.Insert(Math.Min(index, Phases.Count), card);
        }
        else if (current != index)
        {
            Phases.Move(current, index);
        }
    }

    private void SyncConflicts(IReadOnlyList<MesConflict> conflicts, CultureInfo culture)
    {
        Conflicts.Clear();
        foreach (var conflict in conflicts)
        {
            Conflicts.Add(new MesConflictItem(conflict.CommandId, _localization["Mes_Action_" + conflict.Action], FormatTimestamp(conflict.OccurredAtUtc, culture), conflict.Error));
        }

        OnPropertyChanged(nameof(HasConflicts));
    }
}
