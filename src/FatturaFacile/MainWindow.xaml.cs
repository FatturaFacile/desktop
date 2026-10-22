// <copyright file="MainWindow.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;

using FatturaFacile.ViewModels;
using FatturaFacile.Wpf.Common.Helpers;
using FatturaFacile.Wpf.Common.Services;

namespace FatturaFacile;

/// <summary>
/// Main application shell window managing DevExpress docking layout managers, layout serialization,
/// and window lifecycle events.
/// </summary>
public partial class MainWindow : ThemedWindow
{

    /// <summary>
    /// The current docking layout schema version identifier.
    /// </summary>
    private const string CurrentLayoutVersion = "1.1";

    /// <summary>
    /// The main shell view model driving navigation, commands, and active views.
    /// </summary>
    private readonly MainViewModel? _viewModel;

    /// <summary>
    /// The user state service managing visual workspace layout persistence.
    /// </summary>
    private readonly IUserStateService? _userStateService;

    /// <summary>
    /// The captured default XAML layout XML representation used for resetting the workspace.
    /// </summary>
    private string _defaultLayoutXml = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class for XAML designer support or parameterless instantiation.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class with injected dependencies.
    /// Configures layout versioning, docks persistence handlers, and event wiring.
    /// </summary>
    /// <param name="viewModel">The primary shell view model driving navigation and state management.</param>
    /// <param name="userStateService">The persistence service responsible for saving and loading workspace visual layouts.</param>
    public MainWindow(
        MainViewModel     viewModel,
        IUserStateService userStateService)
        : this()
    {
        _viewModel        = viewModel;
        _userStateService = userStateService;
        DataContext       = _viewModel;

        // Sets layout version for DevExpress layout upgrade management
        DXSerializer.SetLayoutVersion(obj: MainDockManager, value: CurrentLayoutVersion);

        // Receives layout reset request from the ViewModel
        _viewModel.RequestResetLayoutView += OnResetLayoutRequestedAsync;

        Loaded  += OnMainWindowLoaded;
        Closing += OnMainWindowClosing;
    }

    /// <summary>
    /// Captures the initial default XAML docking layout snapshot and attempts to restore the user's previously saved visual state.
    /// </summary>
    /// <param name="sender">The source window triggering the loaded event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> event data.</param>
    private void OnMainWindowLoaded(
        object          sender,
        RoutedEventArgs e)
    {
        // Snapshot original XAML layout and restore saved state after rendering completes
        Dispatcher.BeginInvoke(
                               priority: DispatcherPriority.Loaded,
                               method: new Action(async () =>
                                                  {
                                                      _defaultLayoutXml =
                                                          DevExpressLayoutHelper.SaveLayoutToString(rootElement: MainDockManager);

                                                      await LoadPersistedLayoutAsync();
                                                  }));
    }

    /// <summary>
    /// Asynchronously loads and applies the user's persisted layout from <see cref="IUserStateService"/>
    /// if the layout version matches the current layout schema.
    /// </summary>
    /// <returns>A task representing the asynchronous layout loading operation.</returns>
    private async Task LoadPersistedLayoutAsync()
    {
        try
        {
            var state = await _userStateService!.LoadStateAsync();

            // Restore layout only if version matches and XML payload is valid
            if (state.LayoutVersion == CurrentLayoutVersion && !string.IsNullOrWhiteSpace(value: state.VisualState))
            {
                DevExpressLayoutHelper.RestoreLayoutFromString(rootElement: MainDockManager, xmlLayout: state.VisualState);
            }
        }
        catch
        {
            // Restoration failed: retain default initial XAML layout
        }
    }

    /// <summary>
    /// Handles requests to restore the main dock manager's workspace to the original default layout template.
    /// </summary>
    /// <returns>A completed task representing the synchronization boundary.</returns>
    private Task OnResetLayoutRequestedAsync()
    {
        if (!string.IsNullOrWhiteSpace(value: _defaultLayoutXml))
        {
            DevExpressLayoutHelper.RestoreLayoutFromString(rootElement: MainDockManager, xmlLayout: _defaultLayoutXml);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles DevExpress layout upgrade events when deserializing layout states originating from previous schema versions.
    /// </summary>
    /// <param name="sender">The layout serialization manager.</param>
    /// <param name="e">The <see cref="LayoutUpgradeEventArgs"/> event data.</param>
    private void MainDockManager_OnLayoutUpgrade(
        object                 sender,
        LayoutUpgradeEventArgs e)
    {
        // Triggered when serialized layout version in state storage differs from current version
        // Event hook during layout upgrade
    }

    /// <summary>
    /// Handles window closing events, serializing current docking arrangements and persisting them to <see cref="IUserStateService"/>.
    /// </summary>
    /// <param name="sender">The window being closed.</param>
    /// <param name="e">The <see cref="CancelEventArgs"/> event data.</param>
    private async void OnMainWindowClosing(
        object?         sender,
        CancelEventArgs e)
    {
        try
        {
            var visualLayoutXml = DevExpressLayoutHelper.SaveLayoutToString(rootElement: MainDockManager);

            var currentState = new UserState(
                                             LogicalState: string.Empty,
                                             VisualState: visualLayoutXml,
                                             LayoutVersion: CurrentLayoutVersion);

            await _userStateService!.SaveStateAsync(state: currentState);
        }
        catch
        {
            // Suppress save errors to avoid preventing application shutdown
        }
    }

}
