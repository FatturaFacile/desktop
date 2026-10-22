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
/// Finestra principale dell'applicazione con gestione del layout docking e persistenza stato.
/// </summary>
public partial class MainWindow : ThemedWindow
{
    private const string CurrentLayoutVersion = "1.1";
    private readonly MainViewModel? _viewModel;
    private readonly IUserStateService? _userStateService;
    private string _defaultLayoutXml = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(
        MainViewModel viewModel,
        IUserStateService userStateService)
        : this()
    {
        _viewModel = viewModel;
        _userStateService = userStateService;
        DataContext = _viewModel;

        // Imposta la versione per la gestione del layout upgrade di DevExpress
        DXSerializer.SetLayoutVersion(MainDockManager, CurrentLayoutVersion);

        // Riceve la richiesta di reset layout dal ViewModel
        _viewModel.RequestResetLayoutView += OnResetLayoutRequestedAsync;

        Loaded += OnMainWindowLoaded;
        Closing += OnMainWindowClosing;
    }

    private void OnMainWindowLoaded(
        object sender,
        RoutedEventArgs e)
    {
        // Snapshot del layout XAML originale e ripristino stato salvato al termine del rendering
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded, new Action(async () =>
            {
                _defaultLayoutXml = DevExpressLayoutHelper.SaveLayoutToString(MainDockManager);
                await LoadPersistedLayoutAsync();
            }));
    }

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

    private Task OnResetLayoutRequestedAsync()
    {
        if (!string.IsNullOrWhiteSpace(_defaultLayoutXml))
        {
            DevExpressLayoutHelper.RestoreLayoutFromString(MainDockManager, _defaultLayoutXml);
        }

        return Task.CompletedTask;
    }

    private void MainDockManager_OnLayoutUpgrade(
        object sender,
        LayoutUpgradeEventArgs e)
    {
        // Se la versione serializzata nel file JSON non corrisponde alla versione corrente
        // Evento scatenato durante upgrade layout
    }

    private async void OnMainWindowClosing(
        object? sender,
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
