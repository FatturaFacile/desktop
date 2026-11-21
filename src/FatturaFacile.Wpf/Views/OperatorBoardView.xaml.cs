// <copyright file="OperatorBoardView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Threading;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// The MES operator board: a touch-screen view with a one-second timer that advances the live counters.
/// </summary>
internal sealed partial class OperatorBoardView
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

    /// <summary>
    /// Initializes a new instance of the <see cref="OperatorBoardView"/> class.
    /// </summary>
    public OperatorBoardView()
    {
        InitializeComponent();
        _timer.Tick += OnTick;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (DataContext is OperatorBoardViewModel viewModel)
        {
            viewModel.Tick();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => _timer.Start();

    private void OnUnloaded(object sender, RoutedEventArgs e) => _timer.Stop();

    private void OnLotCodeBoxVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // The barcode scanner types into the lot field: give it the focus as soon as the lot panel shows.
        if (e.NewValue is true)
        {
            _ = LotCodeBox.Focus();
        }
    }
}
