// <copyright file="TenantSelectionView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic and code-behind for the modal tenant workspace selection dialog window.
/// </summary>
public partial class TenantSelectionView : Window
{
    /// <summary>
    /// Gets the strongly typed <see cref="TenantSelectionViewModel"/> backing this window.
    /// </summary>
    public TenantSelectionViewModel ViewModel => (TenantSelectionViewModel)DataContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantSelectionView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model dependency.</param>
    public TenantSelectionView(TenantSelectionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
