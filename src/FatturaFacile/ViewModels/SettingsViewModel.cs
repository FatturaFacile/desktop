// <copyright file="SettingsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Ports;
using FatturaFacile.Wpf.Localization;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing application settings, UI localization, visual light/dark theme switching,
/// and tenant switching triggers.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly IThemeManagerService _themeManager;
    private readonly ICurrentTenantService _currentTenantService;

    [ObservableProperty]
    private string _activeTenantName = string.Empty;

    [ObservableProperty]
    private string _activeTenantCurrency = string.Empty;

    /// <summary>
    /// Event raised when the user initiates a tenant switch dialog from settings.
    /// </summary>
    public event Action? RequestTenantSwitch;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="themeManager">The UI theme manager service.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    public SettingsViewModel(
        IThemeManagerService themeManager,
        ICurrentTenantService currentTenantService)
    {
        _themeManager = themeManager;
        _currentTenantService = currentTenantService;
        _activeTenantName = _currentTenantService.CurrentTenant?.Name ?? "N/A";
        _activeTenantCurrency = _currentTenantService.CurrentTenant?.DefaultCurrency ?? "CHF";

        _currentTenantService.TenantChanged += (_, _) =>
        {
            ActiveTenantName = _currentTenantService.CurrentTenant?.Name ?? "N/A";
            ActiveTenantCurrency = _currentTenantService.CurrentTenant?.DefaultCurrency ?? "CHF";
        };
    }

    /// <summary>
    /// Switches the application UI culture language (e.g. "it-IT", "en-US", "de-CH", "fr-CH").
    /// </summary>
    /// <param name="code">The IETF BCP 47 language tag.</param>
    [RelayCommand]
    public void SetLanguage(string code)
    {
        LocalizationManager.Instance.SetCulture(code);
    }

    /// <summary>
    /// Sets the application visual color theme ("light", "dark", or "system").
    /// </summary>
    /// <param name="themeName">The theme keyword name.</param>
    [RelayCommand]
    public void SetTheme(string themeName)
    {
        var theme = themeName.ToLowerInvariant() switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.System
        };
        _themeManager.SetTheme(theme);
    }

    /// <summary>
    /// Raises the <see cref="RequestTenantSwitch"/> event to display the tenant switcher modal dialog.
    /// </summary>
    [RelayCommand]
    public void TriggerTenantSwitch()
    {
        RequestTenantSwitch?.Invoke();
    }
}
