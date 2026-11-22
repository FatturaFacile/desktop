// <copyright file="CountSessionsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Count sessions: status, blind and frozen flags, creation, stock freeze and release (with a reason), assignment,
/// cancellation and the submission of the differences for approval.
/// </summary>
internal sealed partial class CountSessionsViewModel : ObservableObject
{
    private static readonly char[] s_binSeparators = [',', ';', ' ', '\n'];

    private readonly IInventoryCountingApiClient _client;
    private readonly ILocalizationService _localization;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [NotifyCanExecuteChangedFor(nameof(FreezeCommand))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmUnfreezeCommand))]
    [NotifyCanExecuteChangedFor(nameof(AssignCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSessionCommand))]
    [NotifyCanExecuteChangedFor(nameof(SubmitDifferencesCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FreezeCommand))]
    [NotifyCanExecuteChangedFor(nameof(BeginUnfreezeCommand))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmUnfreezeCommand))]
    [NotifyCanExecuteChangedFor(nameof(AssignCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSessionCommand))]
    [NotifyCanExecuteChangedFor(nameof(SubmitDifferencesCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenReconciliationCommand))]
    private CountSessionRow? _selectedSession;

    [ObservableProperty]
    private bool _isCreateOpen;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _newWarehouseId = string.Empty;

    [ObservableProperty]
    private ChoiceItem? _newCountType;

    [ObservableProperty]
    private DateTime? _newScheduledDate = DateTime.Today;

    [ObservableProperty]
    private bool _newFreezesStock;

    [ObservableProperty]
    private bool _newIsBlind = true;

    [ObservableProperty]
    private string _newAssignedTo = string.Empty;

    [ObservableProperty]
    private string _newZoneId = string.Empty;

    [ObservableProperty]
    private string _newBinIds = string.Empty;

    [ObservableProperty]
    private string _newNotes = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmUnfreezeCommand))]
    private bool _isUnfreezeOpen;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmUnfreezeCommand))]
    private string _unfreezeReason = string.Empty;

    [ObservableProperty]
    private string _assignTo = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountSessionsViewModel"/> class.
    /// </summary>
    /// <param name="client">The physical inventory web-api client.</param>
    /// <param name="localization">The localization service.</param>
    public CountSessionsViewModel(IInventoryCountingApiClient client, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(localization);

        _client = client;
        _localization = localization;
        CountTypes = [.. CountingText.CountTypes.Select(type => new ChoiceItem(type, CountingText.CountTypeLabel(type, localization)))];
        _newCountType = CountTypes[0];
    }

    /// <summary>
    /// Raised when the user wants to reconcile a session; the shell shows the reconciliation tab.
    /// </summary>
    public event EventHandler<CountSessionRow>? ReconciliationRequested;

    /// <summary>
    /// Gets the count sessions.
    /// </summary>
    public ObservableCollection<CountSessionRow> Sessions { get; } = [];

    /// <summary>
    /// Gets the count types a new session can have.
    /// </summary>
    public IReadOnlyList<ChoiceItem> CountTypes { get; }

    /// <summary>
    /// Loads the sessions from the web-api, keeping the selection.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        var selectedId = SelectedSession?.Id;
        IsBusy = true;

        try
        {
            var result = await _client.GetSessionsAsync(CancellationToken.None);
            if (!result.IsSuccess || result.Value is null)
            {
                StatusMessage = CountingText.Describe(result, _localization);
                return;
            }

            Sessions.Clear();
            foreach (var session in result.Value)
            {
                Sessions.Add(new CountSessionRow(session, _localization));
            }

            SelectedSession = Sessions.FirstOrDefault(s => string.Equals(s.Id, selectedId, StringComparison.Ordinal));
            StatusMessage = Sessions.Count == 0 ? _localization["Inv_Session_Empty"] : string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Opens the creation form with the defaults.
    /// </summary>
    [RelayCommand]
    public void BeginCreate()
    {
        NewScheduledDate = DateTime.Today;
        NewIsBlind = true;
        IsCreateOpen = true;
    }

    /// <summary>
    /// Closes the creation form without saving.
    /// </summary>
    [RelayCommand]
    public void CancelCreate() => IsCreateOpen = false;

    /// <summary>
    /// Creates the count session from the form and reloads the list.
    /// </summary>
    /// <returns>A task representing the creation.</returns>
    [RelayCommand(CanExecute = nameof(CanCreate))]
    public async Task CreateAsync()
    {
        IsBusy = true;
        var created = false;
        var message = string.Empty;

        try
        {
            var result = await _client.CreateSessionAsync(BuildRequest(), CancellationToken.None);
            created = result.IsSuccess;
            message = created ? _localization["Inv_Session_Created"] : CountingText.Describe(result, _localization);
            IsCreateOpen = !created;
        }
        finally
        {
            IsBusy = false;
        }

        if (created)
        {
            await LoadAsync();
        }

        StatusMessage = message;
    }

    /// <summary>
    /// Freezes the stock of the selected session.
    /// </summary>
    /// <returns>A task representing the freeze.</returns>
    [RelayCommand(CanExecute = nameof(CanFreeze))]
    public Task FreezeAsync() => RunAsync(session => _client.FreezeAsync(session.Id, CancellationToken.None));

    /// <summary>
    /// Opens the reason form that lifts the stock freeze.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanBeginUnfreeze))]
    public void BeginUnfreeze()
    {
        UnfreezeReason = string.Empty;
        IsUnfreezeOpen = true;
    }

    /// <summary>
    /// Closes the unfreeze reason form without saving.
    /// </summary>
    [RelayCommand]
    public void CancelUnfreeze() => IsUnfreezeOpen = false;

    /// <summary>
    /// Lifts the stock freeze of the selected session; the reason is mandatory.
    /// </summary>
    /// <returns>A task representing the release.</returns>
    [RelayCommand(CanExecute = nameof(CanConfirmUnfreeze))]
    public async Task ConfirmUnfreezeAsync()
    {
        var reason = UnfreezeReason.Trim();
        if (await RunAsync(session => _client.UnfreezeAsync(session.Id, reason, CancellationToken.None)))
        {
            IsUnfreezeOpen = false;
        }
    }

    /// <summary>
    /// Assigns the selected session to the entered counter (an empty text clears the assignment).
    /// </summary>
    /// <returns>A task representing the assignment.</returns>
    [RelayCommand(CanExecute = nameof(CanModify))]
    public Task AssignAsync() =>
        RunAsync(session => _client.AssignAsync(session.Id, string.IsNullOrWhiteSpace(AssignTo) ? null : AssignTo.Trim(), CancellationToken.None));

    /// <summary>
    /// Cancels the selected session.
    /// </summary>
    /// <returns>A task representing the cancellation.</returns>
    [RelayCommand(CanExecute = nameof(CanModify))]
    public Task CancelSessionAsync() => RunAsync(session => _client.CancelAsync(session.Id, CancellationToken.None));

    /// <summary>
    /// Submits the differences of the selected session for approval.
    /// </summary>
    /// <returns>A task representing the submission.</returns>
    [RelayCommand(CanExecute = nameof(CanModify))]
    public Task SubmitDifferencesAsync() => RunAsync(session => _client.SubmitDifferencesAsync(session.Id, CancellationToken.None));

    /// <summary>
    /// Asks the shell to show the reconciliation of the selected session.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenReconciliation))]
    public void OpenReconciliation()
    {
        if (SelectedSession is not null)
        {
            ReconciliationRequested?.Invoke(this, SelectedSession);
        }
    }

    private bool CanCreate() => !IsBusy && !string.IsNullOrWhiteSpace(NewWarehouseId) && NewCountType is not null && NewScheduledDate is not null;

    private bool CanFreeze() => !IsBusy && SelectedSession is { CanFreeze: true };

    private bool CanBeginUnfreeze() => SelectedSession is { CanUnfreeze: true };

    private bool CanConfirmUnfreeze() => !IsBusy && IsUnfreezeOpen && SelectedSession is { CanUnfreeze: true } && !string.IsNullOrWhiteSpace(UnfreezeReason);

    private bool CanModify() => !IsBusy && SelectedSession is { CanModify: true };

    private bool CanOpenReconciliation() => SelectedSession is not null;

    private CreateCountSessionRequest BuildRequest()
    {
        var bins = NewBinIds.Split(s_binSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new CreateCountSessionRequest
        {
            WarehouseId = NewWarehouseId.Trim(),
            CountType = NewCountType?.Value ?? CountingText.CountTypes[0],
            ScheduledDate = (NewScheduledDate ?? DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            FreezesStock = NewFreezesStock,
            IsBlind = NewIsBlind,
            AssignedTo = string.IsNullOrWhiteSpace(NewAssignedTo) ? null : NewAssignedTo.Trim(),
            WarehouseZoneId = string.IsNullOrWhiteSpace(NewZoneId) ? null : NewZoneId.Trim(),
            BinIds = bins.Length == 0 ? null : bins,
            Notes = string.IsNullOrWhiteSpace(NewNotes) ? null : NewNotes.Trim(),
        };
    }

    private async Task<bool> RunAsync(Func<CountSessionRow, Task<InventoryCountingResult<bool>>> action)
    {
        if (SelectedSession is null)
        {
            return false;
        }

        IsBusy = true;
        var done = false;
        var message = string.Empty;

        try
        {
            var result = await action(SelectedSession);
            done = result.IsSuccess;
            message = done ? _localization["Inv_Session_ActionDone"] : CountingText.Describe(result, _localization);
        }
        finally
        {
            IsBusy = false;
        }

        if (done)
        {
            await LoadAsync();
        }

        StatusMessage = message;
        return done;
    }
}
