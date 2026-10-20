using System.Windows;

namespace FatturaFacile.Services;

public sealed class ThemeManager
{
    public static ThemeManager Instance => field ??= new ThemeManager();

    public bool IsDarkTheme { get; private set; } = true;

    public void ToggleTheme() => SetTheme(!IsDarkTheme);

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
