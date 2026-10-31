// <copyright file="BackupSettingsView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Controls;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic and code-behind for the database backup management user control view.
/// </summary>
public partial class BackupSettingsView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackupSettingsView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model dependency.</param>
    public BackupSettingsView(BackupSettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
