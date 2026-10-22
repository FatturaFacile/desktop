// <copyright file="ThemeManager.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

namespace FatturaFacile.Services;

/// <summary>
/// Singleton manager responsible for managing application visual themes and toggling between light and dark modes.
/// </summary>
public sealed class ThemeManager
{
    private static ThemeManager? _instance;

    /// <summary>
    /// Gets the singleton instance of the <see cref="ThemeManager"/>.
    /// </summary>
    public static ThemeManager Instance => _instance ??= new ThemeManager();

    /// <summary>
    /// Gets a value indicating whether the dark visual theme is currently active.
    /// </summary>
    public bool IsDarkTheme { get; private set; } = true;

    /// <summary>
    /// Toggles the active application theme between light and dark modes.
    /// </summary>
    public void ToggleTheme() => SetTheme(!IsDarkTheme);

    /// <summary>
    /// Sets the application theme by replacing the corresponding theme resource dictionary in the merged resource hierarchy.
    /// </summary>
    /// <param name="dark"><see langword="true"/> to apply the dark theme; <see langword="false"/> to apply the light theme.</param>
    public void SetTheme(
        bool dark)
    {
        IsDarkTheme = dark;
        var themeUri = dark
            ? new Uri(
                "pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/DarkTheme.xaml", UriKind.Absolute)
            : new Uri(
                "pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/LightTheme.xaml", UriKind.Absolute);

        var appDict = System.Windows.Application.Current.Resources.MergedDictionaries;
        var currentTheme = appDict.FirstOrDefault(d =>
            d.Source != null && (d.Source.OriginalString.Contains("DarkTheme", StringComparison.Ordinal) ||
                                 d.Source.OriginalString.Contains("LightTheme", StringComparison.Ordinal)));

        if (currentTheme != null)
        {
            _ = appDict.Remove(currentTheme);
        }

        appDict.Insert(0, new ResourceDictionary { Source = themeUri });
    }
}
