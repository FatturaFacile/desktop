// <copyright file="LoginView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using FatturaFacile.Desktop.Wpf.ViewModels;

namespace FatturaFacile.Desktop.Wpf.Views;

/// <summary>
/// Interaction logic for LoginView.xaml.
/// </summary>
internal sealed partial class LoginView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginView"/> class.
    /// </summary>
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
        {
            vm.Password = pb.Password;
        }
    }
}
