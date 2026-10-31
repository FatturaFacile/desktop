// <copyright file="InvoicesListView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Controls;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic and code-behind for the invoices list and search user control view.
/// </summary>
public partial class InvoicesListView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoicesListView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model dependency.</param>
    public InvoicesListView(InvoicesListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
