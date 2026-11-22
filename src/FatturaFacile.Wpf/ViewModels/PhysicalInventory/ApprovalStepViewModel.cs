// <copyright file="ApprovalStepViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// One approval level of a count reconciliation. A step can be decided only while it is pending and the session waits
/// for approval, and level 2 only after level 1 was approved; a rejection always needs a reason.
/// </summary>
internal sealed partial class ApprovalStepViewModel : ObservableObject
{
    private const string PendingStatus = "pending";
    private const string ApprovedStatus = "approved";

    private readonly ILocalizationService _localization;
    private readonly Func<ApprovalStepViewModel, Task> _approve;
    private readonly Func<ApprovalStepViewModel, Task> _reject;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPending))]
    [NotifyPropertyChangedFor(nameof(IsApproved))]
    [NotifyPropertyChangedFor(nameof(IsActionable))]
    private string _status = PendingStatus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActionable))]
    private bool _isPresent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActionable))]
    private bool _isSessionAwaitingApproval;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActionable))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusLabel = string.Empty;

    [ObservableProperty]
    private string _requiredRole = string.Empty;

    [ObservableProperty]
    private string _decidedBy = string.Empty;

    [ObservableProperty]
    private string _decidedAt = string.Empty;

    [ObservableProperty]
    private string _recordedNotes = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RejectCommand))]
    private string _rejectReason = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApprovalStepViewModel"/> class.
    /// </summary>
    /// <param name="level">The approval level (1 or 2).</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="approve">Sends the approval of this step.</param>
    /// <param name="reject">Sends the rejection of this step.</param>
    public ApprovalStepViewModel(int level, ILocalizationService localization, Func<ApprovalStepViewModel, Task> approve, Func<ApprovalStepViewModel, Task> reject)
    {
        ArgumentNullException.ThrowIfNull(localization);
        ArgumentNullException.ThrowIfNull(approve);
        ArgumentNullException.ThrowIfNull(reject);

        Level = level;
        _localization = localization;
        _approve = approve;
        _reject = reject;
    }

    /// <summary>
    /// Gets the approval level (1 or 2).
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Gets or sets the step that must be approved before this one can be decided (level 1 for level 2).
    /// </summary>
    public ApprovalStepViewModel? Previous { get; set; }

    /// <summary>
    /// Gets the localized heading of the step.
    /// </summary>
    public string Heading => string.Format(CultureInfo.CurrentCulture, _localization["Inv_Level"], Level);

    /// <summary>
    /// Gets a value indicating whether the step is pending.
    /// </summary>
    public bool IsPending => string.Equals(Status, PendingStatus, StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether the step was approved.
    /// </summary>
    public bool IsApproved => string.Equals(Status, ApprovedStatus, StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether the user can decide the step now: it exists, is pending, the session waits for
    /// approval, no call is running and the previous level (if any) is approved.
    /// </summary>
    public bool IsActionable => IsPresent && IsPending && IsSessionAwaitingApproval && !IsBusy && (Previous is null || Previous.IsApproved);

    /// <summary>
    /// Gets a value indicating whether the step waits for the previous level.
    /// </summary>
    public bool IsWaitingForPrevious => IsPresent && IsPending && IsSessionAwaitingApproval && Previous is { IsApproved: false };

    /// <summary>
    /// Shows the state of the step as served by the web-api.
    /// </summary>
    /// <param name="approval">The approval, or null when the session does not require this level.</param>
    /// <param name="sessionAwaitingApproval">Whether the session is pending approval.</param>
    public void Apply(CountApprovalDto? approval, bool sessionAwaitingApproval)
    {
        IsPresent = approval is not null;
        IsSessionAwaitingApproval = sessionAwaitingApproval;
        Status = approval?.Status ?? PendingStatus;
        StatusLabel = approval is null ? string.Empty : CountingText.ApprovalLabel(approval.Status, _localization);
        RequiredRole = approval?.RequiredRole ?? string.Empty;
        DecidedBy = approval?.DecidedBy ?? string.Empty;
        DecidedAt = approval?.DecidedAt ?? string.Empty;
        RecordedNotes = approval?.Notes ?? string.Empty;
        Notes = string.Empty;
        RejectReason = string.Empty;
        Refresh();
    }

    /// <summary>
    /// Re-evaluates the state that depends on the previous level and notifies the commands.
    /// </summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(IsActionable));
        OnPropertyChanged(nameof(IsWaitingForPrevious));
        ApproveCommand.NotifyCanExecuteChanged();
        RejectCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsSessionAwaitingApprovalChanged(bool value) => Refresh();

    partial void OnIsBusyChanged(bool value) => Refresh();

    partial void OnStatusChanged(string value) => Refresh();

    partial void OnIsPresentChanged(bool value) => Refresh();

    [RelayCommand(CanExecute = nameof(IsActionable))]
    private Task ApproveAsync() => _approve(this);

    [RelayCommand(CanExecute = nameof(CanReject))]
    private Task RejectAsync() => _reject(this);

    private bool CanReject() => IsActionable && !string.IsNullOrWhiteSpace(RejectReason);
}
