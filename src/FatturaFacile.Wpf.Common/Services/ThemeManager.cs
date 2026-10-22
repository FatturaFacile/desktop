// <copyright file="ThemeManager.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Singleton manager responsible for managing application visual themes and toggling between light and dark modes.
/// </summary>
public sealed class ThemeManager
{

    /// <summary>
    /// The singleton instance of the theme manager.
    /// </summary>
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
    public void ToggleTheme() => SetTheme(dark: !IsDarkTheme);

    /// <summary>
    /// Sets the application theme by replacing the corresponding theme resource dictionary in the merged resource hierarchy.
    /// </summary>
    /// <param name="dark"><see langword="true"/> to apply the dark theme; <see langword="false"/> to apply the light theme.</param>
    public void SetTheme(bool dark)
    {
        IsDarkTheme = dark;

        var themeUri = dark
                           ? new Uri(
                                     uriString: "pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/DarkTheme.xaml",
                                     uriKind: UriKind.Absolute)
                           : new Uri(
                                     uriString: "pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/LightTheme.xaml",
                                     uriKind: UriKind.Absolute);

        var appDict = System.Windows.Application.Current.Resources.MergedDictionaries;

        var currentTheme = appDict.FirstOrDefault(predicate: d => d.Source != null &&
                                                                  (d.Source.OriginalString.Contains(
                                                                                                    value: "DarkTheme",
                                                                                                    comparisonType: StringComparison.Ordinal) ||
                                                                   d.Source.OriginalString.Contains(
                                                                                                    value: "LightTheme",
                                                                                                    comparisonType: StringComparison.Ordinal)));

        if (currentTheme != null)
        {
            _ = appDict.Remove(item: currentTheme);
        }

        appDict.Insert(
                       index: 0,
                       item: new ResourceDictionary
                             {
                                 Source = themeUri
                             });
    }

}
