// <copyright file="ThemeManagerService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using FatturaFacile.Application.Ports;
using Microsoft.Win32;
using Syncfusion.SfSkinManager;

namespace FatturaFacile.Wpf.Theming;

/// <summary>
/// Service managing the application's visual theme (System, Light, Dark) using Syncfusion Fluent skin styling
/// and Windows OS registry personalization change events.
/// </summary>
public class ThemeManagerService : IThemeManagerService
{
    private AppTheme _currentTheme = AppTheme.System;

    /// <inheritdoc />
    public AppTheme CurrentTheme => _currentTheme;

    /// <inheritdoc />
    public bool IsDark => ResolveIsDark();

    /// <inheritdoc />
    public event EventHandler<AppTheme>? ThemeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeManagerService"/> class and subscribes to OS user preference changes.
    /// </summary>
    public ThemeManagerService()
    {
        try
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }
        catch
        {
            // Ignore when running in environments without Windows registry event access
        }
    }

    /// <inheritdoc />
    public void SetTheme(AppTheme theme)
    {
        _currentTheme = theme;
        ApplyThemeToApplication();
        ThemeChanged?.Invoke(this, theme);
    }

    /// <summary>
    /// Applies the current theme skin to a specific window instance.
    /// </summary>
    /// <param name="window">The target WPF window.</param>
    public void ApplyThemeToWindow(Window window)
    {
        var themeName = IsDark ? "FluentDark" : "FluentLight";
        try
        {
            SfSkinManager.SetTheme(window, new Theme(themeName));
        }
        catch
        {
            // Ignore if theme styling is unavailable
        }
    }

    private void ApplyThemeToApplication()
    {
        var themeName = IsDark ? "FluentDark" : "FluentLight";

        if (System.Windows.Application.Current != null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    try
                    {
                        SfSkinManager.SetTheme(window, new Theme(themeName));
                    }
                    catch
                    {
                        // Ignore window skinning failure
                    }
                }
            });
        }
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (_currentTheme == AppTheme.System && e.Category == UserPreferenceCategory.General)
        {
            ApplyThemeToApplication();
            ThemeChanged?.Invoke(this, AppTheme.System);
        }
    }

    private bool ResolveIsDark()
    {
        if (_currentTheme == AppTheme.Dark) return true;
        if (_currentTheme == AppTheme.Light) return false;

        // Query Windows Personalize registry
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var val = key?.GetValue("AppsUseLightTheme");
            if (val is int intVal)
            {
                return intVal == 0; // 0 = Dark, 1 = Light
            }
        }
        catch
        {
            // Default to light theme on registry read failure
        }

        return false;
    }
}
