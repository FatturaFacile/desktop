// <copyright file="IThemeManagerService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Provides visual theme management, active palette resolution, and theme transition notifications.
/// </summary>
public interface IThemeManagerService
{
    /// <summary>
    /// Gets the actively configured <see cref="AppTheme"/> preference.
    /// </summary>
    AppTheme CurrentTheme { get; }

    /// <summary>
    /// Gets a value indicating whether the current resolved theme renders in dark mode.
    /// </summary>
    bool IsDark { get; }

    /// <summary>
    /// Applies a new theme preference across the application shells and windows.
    /// </summary>
    /// <param name="theme">The theme option to set.</param>
    void SetTheme(AppTheme theme);

    /// <summary>
    /// Event raised when the visual theme is changed.
    /// </summary>
    event EventHandler<AppTheme>? ThemeChanged;
}
