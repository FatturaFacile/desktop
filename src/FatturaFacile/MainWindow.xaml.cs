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

    private const string CurrentLayoutVersion = "1.1";

    private readonly MainViewModel? _viewModel;

    private readonly IUserStateService? _userStateService;

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

        // Imposta la versione per la gestione del layout upgrade di DevExpress
        DXSerializer.SetLayoutVersion(MainDockManager, CurrentLayoutVersion);

        // Riceve la richiesta di reset layout dal ViewModel
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
        // Snapshot del layout XAML originale e ripristino stato salvato al termine del rendering
        Dispatcher.BeginInvoke(
                               DispatcherPriority.Loaded,
                               new Action(async () =>
                                          {
                                              _defaultLayoutXml =
                                                  DevExpressLayoutHelper.SaveLayoutToString(MainDockManager);

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

            // Ripristina solo se la versione corrisponde e il payload XML è valido
            if (state.LayoutVersion == CurrentLayoutVersion && !string.IsNullOrWhiteSpace(state.VisualState))
            {
                DevExpressLayoutHelper.RestoreLayoutFromString(MainDockManager, state.VisualState);
            }
        }
        catch
        {
            // Ripristino fallito: mantiene inalterato il layout XAML di default
        }
    }

    /// <summary>
    /// Handles requests to restore the main dock manager's workspace to the original default layout template.
    /// </summary>
    /// <returns>A completed task representing the synchronization boundary.</returns>
    private Task OnResetLayoutRequestedAsync()
    {
        if (!string.IsNullOrWhiteSpace(_defaultLayoutXml))
        {
            DevExpressLayoutHelper.RestoreLayoutFromString(MainDockManager, _defaultLayoutXml);
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
        // Se la versione serializzata nel file JSON non corrisponde alla versione corrente
        // Evento scatenato durante upgrade layout
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
            var visualLayoutXml = DevExpressLayoutHelper.SaveLayoutToString(MainDockManager);

            var currentState = new UserState(
                                             LogicalState: string.Empty,
                                             VisualState: visualLayoutXml,
                                             LayoutVersion: CurrentLayoutVersion);

            await _userStateService!.SaveStateAsync(currentState);
        }
        catch
        {
            // Ignora errori di salvataggio per non impedire la chiusura dell'applicazione
        }
    }

}
