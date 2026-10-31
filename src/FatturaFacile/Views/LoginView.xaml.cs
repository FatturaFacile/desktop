// <copyright file="LoginView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic and code-behind for the Windows credentials authentication window.
/// </summary>
public partial class LoginView : Window
{
    /// <summary>
    /// Gets the strongly typed <see cref="LoginViewModel"/> backing this window.
    /// </summary>
    public LoginViewModel ViewModel => (LoginViewModel)DataContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model dependency.</param>
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb)
        {
            ViewModel.Password = pb.Password;
        }
    }
}
