// <copyright file="Bootstrapper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm.ModuleInjection;

namespace FatturaFacile.Wpf.Common;

/// <summary>
/// Legacy bootstrapper for module injection testing and backward compatibility.
/// Coordinates module registration, visual state restoration, navigation bindings, and shell display.
/// </summary>
public class Bootstrapper
{

    /// <summary>
    /// Gets the singleton default instance of the bootstrapper.
    /// </summary>
    public static Bootstrapper? Default { get; protected set; }

    /// <summary>
    /// Instantiates the default bootstrapper and executes the startup pipeline.
    /// </summary>
    public static void Run()
    {
        Default = new Bootstrapper();
        Default.RunCore();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bootstrapper"/> class.
    /// </summary>
    protected Bootstrapper()
    {
    }

    /// <summary>
    /// Gets the global DevExpress module manager used for region-based view model orchestration.
    /// </summary>
    protected static IModuleManager Manager => ModuleManager.DefaultManager;

    /// <summary>
    /// Orchestrates the bootstrapper initialization sequence including type locators,
    /// module registration, state restoration or injection, navigation event subscriptions, and window display.
    /// </summary>
    public virtual void RunCore()
    {
        ConfigureTypeLocators();
        RegisterModules();

        if (!RestoreState())
        {
            InjectModules();
        }

        ConfigureNavigation();
        ShowMainWindow();
    }

    /// <summary>
    /// Configures custom type or view locators for module resolution.
    /// </summary>
    protected virtual void ConfigureTypeLocators()
    {
    }

    /// <summary>
    /// Registers UI modules and configures regional serialization modes with the module manager.
    /// </summary>
    protected virtual void RegisterModules() =>
        Manager.GetRegion(regionName: Common.Regions.Documents).VisualSerializationMode = VisualSerializationMode.PerKey;

    /// <summary>
    /// Attempts to restore previously persisted visual and logical layout state.
    /// </summary>
    /// <returns><see langword="true"/> if state was successfully restored; otherwise,
    /// <see langword="false"/> to trigger default injection.</returns>
    protected virtual bool RestoreState() => false;

    /// <summary>
    /// Injects default modules into regions when prior state restoration was not performed.
    /// </summary>
    protected virtual void InjectModules()
    {
    }

    /// <summary>
    /// Subscribes to navigation and document region events to maintain synchronization
    /// between active tabs and navigation elements.
    /// </summary>
    protected virtual void ConfigureNavigation()
    {
        Manager.GetEvents(regionName: Common.Regions.Navigation).Navigation += OnNavigation;
        Manager.GetEvents(regionName: Common.Regions.Documents).Navigation  += OnDocumentsNavigation;
    }

    /// <summary>
    /// Instantiates and displays the primary application shell window.
    /// </summary>
    protected virtual void ShowMainWindow()
    {
    }

    /// <summary>
    /// Handles navigation events from the navigation region and synchronizes the document region.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The navigation event arguments.</param>
    private static void OnNavigation(object? sender, NavigationEventArgs e)
    {
        if (e.NewViewModelKey == null)
        {
            return;
        }

        Manager.InjectOrNavigate(regionName: Common.Regions.Documents, key: e.NewViewModelKey);
        SyncDocumentActiveState(activeKey: e.NewViewModelKey);
    }

    /// <summary>
    /// Handles navigation events from the document region and synchronizes the navigation region.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The navigation event arguments.</param>
    private static void OnDocumentsNavigation(object? sender, NavigationEventArgs e)
    {
        if (e.NewViewModelKey == null)
        {
            return;
        }

        Manager.Navigate(regionName: Common.Regions.Navigation, key: e.NewViewModelKey);
        SyncDocumentActiveState(activeKey: e.NewViewModelKey);
    }

    /// <summary>
    /// Updates the active state of document modules within the document region.
    /// </summary>
    /// <param name="activeKey">The key of the active view model.</param>
    private static void SyncDocumentActiveState(string activeKey)
    {
        var docRegion = Manager.GetRegion(regionName: Common.Regions.Documents);

        if (docRegion == null)
        {
            return;
        }

        foreach (var vm in docRegion.ViewModels)
        {
            if (vm is IDocumentModule doc)
            {
                doc.IsActive = (docRegion.GetKey(viewModel: vm) == activeKey);
            }
        }
    }

}
