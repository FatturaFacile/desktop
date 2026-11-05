// <copyright file="MainWindow.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.ViewModels;
using Syncfusion.Windows.Shared;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml, hosting ERP modules and tenant switching.
/// </summary>
public partial class MainWindow : ChromelessWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The root application view model.</param>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
