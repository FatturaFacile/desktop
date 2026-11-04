// <copyright file="MainWindow.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using FatturaFacile.Desktop.Wpf.ViewModels;

namespace FatturaFacile.Desktop.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
internal sealed partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class with the shell view model.
    /// </summary>
    /// <param name="viewModel">The root shell view model.</param>
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
