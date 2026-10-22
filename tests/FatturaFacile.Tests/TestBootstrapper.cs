// <copyright file="TestBootstrapper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using Common;
using DevExpress.Mvvm.ModuleInjection;
using UserControl = System.Windows.Controls.UserControl;

namespace FatturaFacile.Tests;

/// <summary>
/// Test bootstrapper used to configure and initialize the application
/// infrastructure required by unit and functional tests.
/// </summary>
public class TestBootstrapper : Bootstrapper
{
    /// <summary>
    /// Gets the current test bootstrapper instance.
    /// </summary>
    public static new TestBootstrapper? Default
    {
        get => Bootstrapper.Default as TestBootstrapper;
        protected set => Bootstrapper.Default = value;
    }

    /// <summary>
    /// Creates and initializes a new <see cref="TestBootstrapper"/> instance.
    /// </summary>
    /// <param name="showRealWindow">
    /// A value indicating whether the real application window should be displayed.
    /// </param>
    public static void TestRun(bool showRealWindow)
    {
        Default = new TestBootstrapper(showRealWindow);
        Default.RunCore();
    }

    private readonly bool showRealWindow;

    /// <summary>
    /// Gets the application's main window instance.
    /// </summary>
    public Window? MainWindow { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBootstrapper"/> class.
    /// </summary>
    /// <param name="showRealWindow">
    /// A value indicating whether the real application window should be displayed.
    /// </param>
    public TestBootstrapper(bool showRealWindow)
    {
        this.showRealWindow = showRealWindow;
    }

    /// <summary>
    /// Registers all modules required for testing.
    /// </summary>
    protected override void RegisterModules()
    {
        base.RegisterModules();

        Manager.Register(
            Regions.MainWindow,
            new Module(global::Common.Modules.Main, () => new object(), typeof(UserControl)));

        Manager.Register(
            Regions.Navigation,
            new Module(global::Common.Modules.Module1, () => new object(), typeof(UserControl)));

        Manager.Register(
            Regions.Navigation,
            new Module(global::Common.Modules.Module2, () => new object(), typeof(UserControl)));

        Manager.Register(
            Regions.Documents,
            new Module(
                global::Common.Modules.Module1,
                () => new TestDocumentModule(global::Common.Modules.Module1, "Module 1"),
                typeof(UserControl)));

        Manager.Register(
            Regions.Documents,
            new Module(
                global::Common.Modules.Module2,
                () => new TestDocumentModule(global::Common.Modules.Module2, "Module 2"),
                typeof(UserControl)));
    }

    /// <summary>
    /// Injects the initial modules into their corresponding regions.
    /// </summary>
    protected override void InjectModules()
    {
        base.InjectModules();

        Manager.Inject(Regions.MainWindow, global::Common.Modules.Main);
        Manager.Inject(Regions.Navigation, global::Common.Modules.Module1);
        Manager.Inject(Regions.Navigation, global::Common.Modules.Module2);
    }

    /// <summary>
    /// Overrides the default main window display behavior for tests.
    /// </summary>
    /// <remarks>
    /// Test executions normally do not show a user interface. Functional
    /// tests may override this behavior if visual interaction is required.
    /// </remarks>
    protected override void ShowMainWindow()
    {
    }
}
