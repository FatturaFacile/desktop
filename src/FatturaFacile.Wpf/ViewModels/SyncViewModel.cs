// <copyright file="SyncViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.Sync;
using Microsoft.Extensions.Options;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the web-api sign-in dialog and the "Sync Now" command of the shell.
/// </summary>
internal sealed partial class SyncViewModel : ObservableObject
{
    private readonly ISyncApiClient _api;
    private readonly SyncCoordinator _coordinator;
    private readonly ILocalizationService _localization;

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncViewModel"/> class.
    /// </summary>
    /// <param name="api">The web-api client.</param>
    /// <param name="coordinator">The sync coordinator.</param>
    /// <param name="options">The sync options.</param>
    /// <param name="localization">The localization service.</param>
    public SyncViewModel(
        ISyncApiClient api,
        SyncCoordinator coordinator,
        IOptions<SyncOptions> options,
        ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(options);

        _api = api;
        _coordinator = coordinator;
        _localization = localization;
        IsAvailable = options.Value.IsEnabled;
        _isConnected = api.IsAuthenticated;
        _coordinator.Completed += (_, _) => OnPassCompleted();
    }

    /// <summary>
    /// Gets a value indicating whether synchronization is configured (a web-api URL is set).
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Opens the sync dialog.
    /// </summary>
    [RelayCommand]
    public void OpenDialog()
    {
        Password = string.Empty;
        IsDialogOpen = true;
    }

    /// <summary>
    /// Closes the sync dialog.
    /// </summary>
    [RelayCommand]
    public void CloseDialog()
    {
        Password = string.Empty;
        IsDialogOpen = false;
    }

    /// <summary>
    /// Signs in against the web-api and runs a first pass.
    /// </summary>
    /// <returns>A task representing the sign-in.</returns>
    [RelayCommand]
    public async Task ConnectAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            var accepted = await _api.LoginAsync(Email, Password, CancellationToken.None);
            Password = string.Empty;
            IsConnected = accepted;

            if (!accepted)
            {
                StatusMessage = _localization["Sync_LoginFailed"];
                return;
            }

            await RunPassAsync();
        }
        catch (HttpRequestException ex)
        {
            StatusMessage = Format("Sync_Error", ex.Message);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = Format("Sync_Error", _localization["Sync_Timeout"]);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Runs a sync pass now.
    /// </summary>
    /// <returns>A task representing the pass.</returns>
    [RelayCommand]
    public async Task SyncNowAsync()
    {
        IsBusy = true;

        try
        {
            await RunPassAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Ends the web-api session; local data stays untouched.
    /// </summary>
    [RelayCommand]
    public void Disconnect()
    {
        _api.Logout();
        IsConnected = false;
        StatusMessage = _localization["Sync_NotConnected"];
    }

    private async Task RunPassAsync()
    {
        try
        {
            var result = await _coordinator.RunNowAsync(CancellationToken.None);
            Describe(result);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = Format("Sync_Error", _localization["Sync_Timeout"]);
        }
    }

    private void OnPassCompleted()
    {
        if (_coordinator.LastResult is { } result)
        {
            Describe(result);
        }
    }

    private void Describe(SyncRunResult result)
    {
        IsConnected = _api.IsAuthenticated;

        if (result.AuthRequired)
        {
            StatusMessage = _localization["Sync_AuthRequired"];
            return;
        }

        var summary = string.Format(
            CultureInfo.CurrentCulture,
            _localization["Sync_Result"],
            result.Pushed,
            result.Pulled,
            result.Conflicts,
            result.Failed);

        StatusMessage = result.Errors.Count > 0 ? $"{summary} {result.Errors[0]}" : summary;
    }

    private string Format(string key, string argument) =>
        string.Format(CultureInfo.CurrentCulture, _localization[key], argument);
}
