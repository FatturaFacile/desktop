// <copyright file="SecurityViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Security;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Security module: local encryption status, session lock settings, offline audit queue and PII unmasking.
/// </summary>
internal sealed partial class SecurityViewModel : ObservableObject
{
    private const int RecentEntries = 100;

    private static readonly Dictionary<SyncSendResult, string> _unmaskFailureKeys = new()
    {
        [SyncSendResult.Unauthorized] = "Sync_AuthRequired",
        [SyncSendResult.Transient] = "Security_Pii_Offline",
    };

    private readonly ILocalEncryptionStatusService _encryptionStatus;
    private readonly ISecuritySettingsStore _settingsStore;
    private readonly ISecurityApiClient _api;
    private readonly IServiceScopeFactory _scopes;
    private readonly ILocalizationService _localization;
    private LocalEncryptionStatus? _encryption;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _idleLockEnabled = true;

    [ObservableProperty]
    private int _lockTimeoutMinutes = SecuritySettings.DefaultLockTimeoutMinutes;

    [ObservableProperty]
    private bool _requirePinToUnlock = true;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private int _syncedCount;

    [ObservableProperty]
    private string _lastSyncText = string.Empty;

    [ObservableProperty]
    private string _chainStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _chainIsIntact = true;

    [ObservableProperty]
    private bool _isUnmaskDialogOpen;

    [ObservableProperty]
    private string _unmaskEntityType = string.Empty;

    [ObservableProperty]
    private string _unmaskEntityId = string.Empty;

    [ObservableProperty]
    private string _unmaskField = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(ConfirmUnmaskCommand))]
    [NotifyPropertyChangedFor(nameof(JustificationError))]
    [ObservableProperty]
    private string _unmaskJustification = string.Empty;

    [ObservableProperty]
    private string _unmaskStatusMessage = string.Empty;

    [ObservableProperty]
    private string _revealedValue = string.Empty;

    [ObservableProperty]
    private bool _hasRevealedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityViewModel"/> class.
    /// </summary>
    /// <param name="encryptionStatus">The local encryption status service.</param>
    /// <param name="settingsStore">The security settings store.</param>
    /// <param name="api">The web-api security client.</param>
    /// <param name="scopes">The scope factory (the audit queue is scoped).</param>
    /// <param name="localization">The localization service.</param>
    public SecurityViewModel(
        ILocalEncryptionStatusService encryptionStatus,
        ISecuritySettingsStore settingsStore,
        ISecurityApiClient api,
        IServiceScopeFactory scopes,
        ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(localization);

        _encryptionStatus = encryptionStatus;
        _settingsStore = settingsStore;
        _api = api;
        _scopes = scopes;
        _localization = localization;
        localization.CultureChanged += (_, _) => OnPropertyChanged(string.Empty);
    }

    /// <summary>
    /// Gets the recent entries of the offline audit queue.
    /// </summary>
    public ObservableCollection<AuditEntryRow> Entries { get; } = [];

    /// <summary>
    /// Gets the allowed range text of the lock timeout.
    /// </summary>
    public string LockHint => string.Format(CultureInfo.CurrentCulture, _localization["Security_Lock_Hint"], SecuritySettings.MinLockTimeoutMinutes, SecuritySettings.MaxLockTimeoutMinutes);

    /// <summary>
    /// Gets a value indicating whether the local data is protected at rest.
    /// </summary>
    public bool IsProtected => _encryption?.IsProtected ?? false;

    /// <summary>
    /// Gets the localized summary of the encryption state.
    /// </summary>
    public string EncryptionSummary => _localization[IsProtected ? "Security_Encryption_Protected" : "Security_Encryption_Attention"];

    /// <summary>
    /// Gets the localized "database file encrypted" value.
    /// </summary>
    public string DatabaseEncryptedText => YesNo(_encryption?.DatabaseFileEncrypted ?? false);

    /// <summary>
    /// Gets the localized "sensitive fields encrypted" value.
    /// </summary>
    public string FieldEncryptionText => YesNo(_encryption?.FieldEncryptionActive ?? false);

    /// <summary>
    /// Gets the localized "key protected by DPAPI" value.
    /// </summary>
    public string KeyDpapiText => YesNo(_encryption?.KeyProtectedByDpapi ?? false);

    /// <summary>
    /// Gets the key protection scheme name.
    /// </summary>
    public string ProtectionScheme => _encryption?.ProtectionScheme ?? string.Empty;

    /// <summary>
    /// Gets the path of the local database.
    /// </summary>
    public string DatabasePath => _encryption?.DatabasePath ?? string.Empty;

    /// <summary>
    /// Gets the validation message of the justification (empty when valid or still untouched).
    /// </summary>
    public string JustificationError => UnmaskJustification.Length == 0 || PiiUnmaskRules.IsJustificationValid(UnmaskJustification)
        ? string.Empty
        : string.Format(CultureInfo.CurrentCulture, _localization["Security_Pii_JustificationShort"], PiiUnmaskRules.MinimumJustificationLength);

    /// <summary>
    /// Gets the label of the justification field (with the minimum length).
    /// </summary>
    public string JustificationLabel => string.Format(CultureInfo.CurrentCulture, _localization["Security_Pii_Justification"], PiiUnmaskRules.MinimumJustificationLength);

    /// <summary>
    /// Loads status, settings and queue.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        _encryption = _encryptionStatus.GetStatus();
        OnPropertyChanged(string.Empty);

        var settings = _settingsStore.Load();
        IdleLockEnabled = settings.IdleLockEnabled;
        LockTimeoutMinutes = settings.LockTimeoutMinutes;
        RequirePinToUnlock = settings.RequirePinToUnlock;

        await RefreshQueueAsync();
    });

    /// <summary>
    /// Opens the PII unmasking confirmation dialog.
    /// </summary>
    [RelayCommand]
    public void OpenUnmaskDialog()
    {
        UnmaskJustification = string.Empty;
        UnmaskStatusMessage = string.Empty;
        HideRevealed();
        IsUnmaskDialogOpen = true;
    }

    /// <summary>
    /// Closes the PII unmasking confirmation dialog and forgets any revealed value.
    /// </summary>
    [RelayCommand]
    public void CancelUnmask()
    {
        UnmaskJustification = string.Empty;
        HideRevealed();
        IsUnmaskDialogOpen = false;
    }

    /// <summary>
    /// Forgets the revealed value.
    /// </summary>
    [RelayCommand]
    public void HideRevealed()
    {
        RevealedValue = string.Empty;
        HasRevealedValue = false;
    }

    private static string Format(DateTime? utc) => utc is { } value ? value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture) : string.Empty;

    private string YesNo(bool value) => _localization[value ? "Security_Yes" : "Security_No"];

    private string Describe(string key, params object[] arguments) => string.Format(CultureInfo.CurrentCulture, _localization[key], arguments);

    private Task RunAsync(Func<Task> operation)
    {
        return Execute();

        async Task Execute()
        {
            IsBusy = true;

            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private Task SaveSettingsAsync() => RunAsync(async () =>
    {
        var settings = new SecuritySettings
        {
            IdleLockEnabled = IdleLockEnabled,
            LockTimeoutMinutes = LockTimeoutMinutes,
            RequirePinToUnlock = RequirePinToUnlock,
        }.Normalize();

        await AppendAuditAsync(new AuditEventInput(
            "security_settings",
            "device",
            "security_settings_changed",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["idle_lock_enabled"] = settings.IdleLockEnabled,
                ["lock_timeout_minutes"] = settings.LockTimeoutMinutes,
                ["require_pin_to_unlock"] = settings.RequirePinToUnlock,
            }));

        _settingsStore.Save(settings);
        LockTimeoutMinutes = settings.LockTimeoutMinutes;
        StatusMessage = _localization["Security_Lock_Saved"];
        await RefreshQueueAsync();
    });

    [RelayCommand]
    private Task VerifyChainAsync() => RunAsync(async () =>
    {
        await using var scope = _scopes.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ILocalAuditQueue>().VerifyChainAsync(CancellationToken.None);
        ApplyVerification(result);
    });

    [RelayCommand]
    private Task SyncAuditNowAsync() => RunAsync(async () =>
    {
        if (!_api.IsAuthenticated)
        {
            StatusMessage = _localization["Security_Queue_NotConnected"];
            return;
        }

        await using (var scope = _scopes.CreateAsyncScope())
        {
            var result = await scope.ServiceProvider.GetRequiredService<AuditQueueSynchronizer>().RunAsync(CancellationToken.None);
            StatusMessage = result switch
            {
                { ChainBroken: true } => Describe("Security_Queue_ChainBroken", "?", result.Error ?? string.Empty),
                { AuthRequired: true } => _localization["Sync_AuthRequired"],
                { Error: not null } => Describe("Security_Queue_Error", result.Error),
                _ => Describe("Security_Queue_SyncResult", result.Uploaded, result.Remaining),
            };
        }

        await RefreshQueueAsync();
    });

    [RelayCommand(CanExecute = nameof(CanConfirmUnmask))]
    private Task ConfirmUnmaskAsync() => RunAsync(async () =>
    {
        HideRevealed();

        if (string.IsNullOrWhiteSpace(UnmaskEntityType) || string.IsNullOrWhiteSpace(UnmaskEntityId) || string.IsNullOrWhiteSpace(UnmaskField))
        {
            UnmaskStatusMessage = _localization["Security_Pii_FieldsRequired"];
            return;
        }

        var request = new PiiUnmaskRequest(UnmaskEntityType.Trim(), UnmaskEntityId.Trim(), UnmaskField.Trim(), UnmaskJustification.Trim());

        // Fail closed: the request is recorded in the signed local chain before anything is revealed.
        await AppendAuditAsync(new AuditEventInput(
            request.EntityType,
            request.EntityId,
            "pii_unmask_requested",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["field"] = request.Field,
                ["justification"] = request.Justification,
            }));

        var outcome = await _api.UnmaskPiiAsync(request, CancellationToken.None);
        var revealed = outcome.Result == SyncSendResult.Success;
        RevealedValue = revealed ? outcome.Value ?? string.Empty : string.Empty;
        HasRevealedValue = revealed;
        var failure = _unmaskFailureKeys.TryGetValue(outcome.Result, out var key)
            ? _localization[key]
            : Describe("Security_Pii_Denied", outcome.Error ?? string.Empty);
        UnmaskStatusMessage = revealed ? string.Empty : failure;

        UnmaskJustification = string.Empty;
        await RefreshQueueAsync();
    });

    private bool CanConfirmUnmask() => !IsBusy && PiiUnmaskRules.IsJustificationValid(UnmaskJustification);

    partial void OnIsBusyChanged(bool value) => ConfirmUnmaskCommand.NotifyCanExecuteChanged();

    private async Task AppendAuditAsync(AuditEventInput input)
    {
        await using var scope = _scopes.CreateAsyncScope();
        _ = await scope.ServiceProvider.GetRequiredService<ILocalAuditQueue>().AppendAsync(input, CancellationToken.None);
    }

    private async Task RefreshQueueAsync()
    {
        await using var scope = _scopes.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<ILocalAuditQueue>();

        var summary = await queue.GetSummaryAsync(CancellationToken.None);
        PendingCount = summary.Pending;
        SyncedCount = summary.Synced;
        LastSyncText = summary.LastSyncedAt is { } last ? Format(last) : _localization["Security_Queue_Never"];

        Entries.Clear();
        foreach (var entry in await queue.GetRecentAsync(RecentEntries, CancellationToken.None))
        {
            Entries.Add(ToRow(entry));
        }

        ApplyVerification(await queue.VerifyChainAsync(CancellationToken.None));
    }

    private void ApplyVerification(AuditChainVerification result)
    {
        ChainIsIntact = result.IsValid;
        ChainStatusMessage = result.IsValid
            ? Describe("Security_Queue_ChainOk", result.EntriesChecked)
            : Describe("Security_Queue_ChainBroken", result.BrokenAtSequence ?? 0, result.Reason ?? string.Empty);
    }

    private AuditEntryRow ToRow(LocalAuditEntry entry) => new(
        entry.SequenceNumber,
        Format(entry.OccurredAt),
        $"{entry.EntityType}/{entry.EntityId}",
        entry.Action,
        _localization[entry.Status == AuditQueueStatus.Synced ? "Security_Status_Synced" : "Security_Status_Pending"],
        Format(entry.ServerReceivedAt));
}
