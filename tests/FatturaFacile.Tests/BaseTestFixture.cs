// <copyright file="BaseTestFixture.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using DevExpress.Mvvm.ModuleInjection;
using DevExpress.Xpf.Core;

using WpfApplication = System.Windows.Application;

namespace FatturaFacile.Tests;

/// <summary>
/// Provides a common base class for unit and functional test fixtures that
/// require WPF application resources and module manager initialization.
/// </summary>
public abstract class BaseTestFixture
{
    /// <summary>
    /// Gets the module manager used by the tests.
    /// </summary>
    public IModuleManager Manager => ModuleManager.DefaultManager;

    /// <summary>
    /// Gets a value indicating whether functional testing mode is enabled.
    /// </summary>
    protected virtual bool IsFunctionalTesting => false;

    /// <summary>
    /// Initializes the test environment before each test execution.
    /// </summary>
    /// <remarks>
    /// This method ensures that WPF application resources are available,
    /// creates a new module manager instance, and starts the test bootstrapper.
    /// </remarks>
    [SetUp]
    public void SetUp()
    {
        EnsureApplicationResources();
        ModuleManager.DefaultManager = new ModuleManager(false, true);
        TestBootstrapper.TestRun(IsFunctionalTesting);
    }

    /// <summary>
    /// Cleans up the test environment after each test execution.
    /// </summary>
    [TearDown]
    public void TearDown() => ModuleManager.DefaultManager = null!;

    /// <summary>
    /// Processes all pending dispatcher messages.
    /// </summary>
    /// <remarks>
    /// This helper method allows asynchronous UI operations and bindings
    /// to complete during functional tests.
    /// </remarks>
    protected void DoEvents()
    {
        DispatcherHelper.DoEvents();
    }

    /// <summary>
    /// Ensures that the WPF application instance and required resource
    /// dictionaries are loaded.
    /// </summary>
    /// <remarks>
    /// Resources are loaded only once and reused across all test executions.
    /// </remarks>
    private static void EnsureApplicationResources()
    {
        if (WpfApplication.Current == null)
        {
            _ = new WpfApplication();
        }

        if (WpfApplication.Current != null &&
            WpfApplication.Current.Resources.MergedDictionaries.Count == 0)
        {
            WpfApplication.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri(
                        "pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/DarkTheme.xaml",
                        UriKind.Absolute)
                });

            WpfApplication.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri(
                        "pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/Icons.xaml",
                        UriKind.Absolute)
                });

            WpfApplication.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri(
                        "pack://application:,,,/FatturaFacile.Wpf.Common;component/Themes/Styles.xaml",
                        UriKind.Absolute)
                });

            WpfApplication.Current.Resources["BooleanToVisibilityConverter"] =
                new System.Windows.Controls.BooleanToVisibilityConverter();

            WpfApplication.Current.Resources["InverseBooleanConverter"] =
                new FatturaFacile.Markup.InverseBooleanConverter();

            WpfApplication.Current.Resources["StringToVisibilityConverter"] =
                new FatturaFacile.Markup.StringToVisibilityConverter();
        }
    }
}
