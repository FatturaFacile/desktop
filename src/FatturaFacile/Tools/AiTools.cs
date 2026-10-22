using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DevExpress.AIIntegration;
using DevExpress.Xpf.Core;
using Application = System.Windows.Application;

namespace AIDemo.Tools {
    public static class AiTools {
        static readonly HashSet<string> availableThemes = Theme.Themes.Select(x => x.Name).ToHashSet();

        static ThemedWindow GetMainWindow() => Application.Current.Windows.OfType<ThemedWindow>().FirstOrDefault(w => w.IsActive);

        [AIIntegrationTool]
        [Description("Toggle the application between full-screen and windowed modes.")]
        public static string SwitchFullScreenMode() {
            var win = GetMainWindow();
            win.WindowState = win.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            var state = win.WindowState == WindowState.Maximized ? "full-screen" : "windowed";
            return $"Window state: {state}";
        }

        [AIIntegrationTool]
        [Description("Close the application.")]
        public static void CloseApplication() {
            var win = GetMainWindow();
            win.Close();
        }

        [AIIntegrationTool]
        [Description("Gets information about available application themes. Always use this before changing the theme.")]
        public static object GetThemesInfo() => new {
            CurrentTheme = ApplicationThemeHelper.ApplicationThemeName,
            AvailableThemes = availableThemes
        };

        [AIIntegrationTool]
        [Description("Changes the application theme.")]
        public static object ChangeTheme(
            [Description("The name of the theme to apply.")]
            string themeName) {
            if(!availableThemes.Contains(themeName))
                return new { success = false, error = $"Theme '{themeName}' is not available." };

            ApplicationThemeHelper.ApplicationThemeName = themeName;
            return new { success = true, appliedTheme = themeName };
        }
    }
}
