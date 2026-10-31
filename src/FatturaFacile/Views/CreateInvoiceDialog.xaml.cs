// <copyright file="CreateInvoiceDialog.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic and code-behind for the modal invoice creation dialog window.
/// </summary>
public partial class CreateInvoiceDialog : Window
{
    /// <summary>
    /// Gets the strongly typed <see cref="CreateInvoiceViewModel"/> backing this window.
    /// </summary>
    public CreateInvoiceViewModel ViewModel => (CreateInvoiceViewModel)DataContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInvoiceDialog"/> class.
    /// </summary>
    /// <param name="viewModel">The view model dependency.</param>
    public CreateInvoiceDialog(CreateInvoiceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.InvoiceCreated += () =>
        {
            DialogResult = true;
            Close();
        };

        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
