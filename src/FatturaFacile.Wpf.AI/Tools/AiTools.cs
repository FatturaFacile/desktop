// <copyright file="AiTools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Windows;

using DevExpress.AIIntegration;
using DevExpress.Xpf.Core;

namespace FatturaFacile.Wpf.AI.Tools;

/// <summary>
/// General application automation tools exposed to AI agents, including theme switching and window controls.
/// </summary>
public static class AiTools
{

    private static readonly HashSet<string> availableThemes = Theme.Themes.Select(selector: x => x.Name).ToHashSet();

    private static ThemedWindow GetMainWindow() => System.Windows.Application.Current.Windows.OfType<ThemedWindow>()
                                                         .FirstOrDefault(predicate: w => w.IsActive);

    /// <summary>
    /// Toggles the main application window between maximized (full-screen) and normal restored state.
    /// </summary>
    /// <returns>A status string describing the resulting window state.</returns>
    [AIIntegrationTool]
    [Description(description: "Toggle the application between full-screen and windowed modes.")]
    public static string SwitchFullScreenMode()
    {
        var win = GetMainWindow();

        win.WindowState = win.WindowState == WindowState.Maximized
                              ? WindowState.Normal
                              : WindowState.Maximized;

        var state = win.WindowState == WindowState.Maximized ? "full-screen" : "windowed";

        return $"Window state: {state}";
    }

    /// <summary>
    /// Gracefully closes the active main application window.
    /// </summary>
    [AIIntegrationTool]
    [Description(description: "Close the application.")]
    public static void CloseApplication()
    {
        var win = GetMainWindow();
        win.Close();
    }

    /// <summary>
    /// Retrieves current theme configuration and the complete set of supported DevExpress visual theme names.
    /// </summary>
    /// <returns>An object declaring active and available themes.</returns>
    [AIIntegrationTool]
    [Description(description: "Gets information about available application themes. Always use this before changing the theme.")]
    public static object GetThemesInfo() => new
                                            {
                                                CurrentTheme    = ApplicationThemeHelper.ApplicationThemeName,
                                                AvailableThemes = availableThemes,
                                            };

    /// <summary>
    /// Applies a selected visual appearance theme across the running WPF application.
    /// </summary>
    /// <param name="themeName">The canonical name of the target DevExpress theme.</param>
    /// <returns>An object indicating success status or error details.</returns>
    [AIIntegrationTool]
    [Description(                                 description: "Changes the application theme.")]
    public static object ChangeTheme([Description(description: "The name of the theme to apply.")] string themeName)
    {
        if (!availableThemes.Contains(item: themeName))
        {
            return new
                   {
                       success = false,
                       error   = $"Theme '{themeName}' is not available.",
                   };
        }

        ApplicationThemeHelper.ApplicationThemeName = themeName;

        return new
               {
                   success      = true,
                   appliedTheme = themeName,
               };
    }

}
