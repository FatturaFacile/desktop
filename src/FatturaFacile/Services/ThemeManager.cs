using System.Windows;

namespace FatturaFacile.Services;

/// <summary>
    /// Represents theme manager.
    /// </summary>
    public sealed class ThemeManager
    {
    /// <summary>
    /// Gets the instance.
    /// </summary>
public static ThemeManager Instance => field ??= new ThemeManager();

/// <summary>
/// Gets a value indicating whether dark theme.
/// </summary>
    public bool IsDarkTheme { get; private set; } = true;

/// <summary>
/// Toggle Theme.
/// </summary>
    public void ToggleTheme() => SetTheme(!IsDarkTheme);

/// <summary>
/// Set Theme.
/// </summary>
/// <param name="dark">The dark.</param>
    public void SetTheme(bool dark)
    {
        IsDarkTheme = dark;
        var themeUri = dark
            ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

        var appDict = System.Windows.Application.Current.Resources.MergedDictionaries;
        var currentTheme = appDict.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("DarkTheme", StringComparison.Ordinal) || d.Source.OriginalString.Contains("LightTheme", StringComparison.Ordinal)));

        if (currentTheme != null)
        {
            _ = appDict.Remove(currentTheme);
        }

        appDict.Insert(0, new ResourceDictionary { Source = themeUri });
    }
}
