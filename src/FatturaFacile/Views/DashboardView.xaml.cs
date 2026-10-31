// <copyright file="DashboardView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Controls;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic and code-behind for the executive KPI dashboard user control view.
/// </summary>
public partial class DashboardView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model dependency.</param>
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
