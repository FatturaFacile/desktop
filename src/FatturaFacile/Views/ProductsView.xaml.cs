// <copyright file="ProductsView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Controls;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic and code-behind for the products and services catalog user control view.
/// </summary>
public partial class ProductsView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model dependency.</param>
    public ProductsView(ProductsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
