// <copyright file="MainShellView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using FatturaFacile.Application.Features.Tenants;
using FatturaFacile.Application.Ports;
using FatturaFacile.Wpf.Theming;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Views;

/// <summary>
/// Interaction logic and code-behind for the primary application shell window,
/// hosting sub-views and handling dialog presentation.
/// </summary>
public partial class MainShellView : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ThemeManagerService _themeManager;

    /// <summary>
    /// Gets the strongly typed <see cref="MainShellViewModel"/> backing this window.
    /// </summary>
    public MainShellViewModel ViewModel => (MainShellViewModel)DataContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainShellView"/> class.
    /// </summary>
    /// <param name="viewModel">The main shell view model dependency.</param>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <param name="themeManager">The theme manager service.</param>
    public MainShellView(
        MainShellViewModel viewModel,
        IServiceProvider serviceProvider,
        ThemeManagerService themeManager)
    {
        InitializeComponent();
        DataContext = viewModel;
        _serviceProvider = serviceProvider;
        _themeManager = themeManager;

        viewModel.RequestTenantSwitch += OnRequestTenantSwitch;
        viewModel.RequestNewInvoiceDialog += OnRequestNewInvoiceDialog;

        Loaded += (_, _) =>
        {
            _themeManager.ApplyThemeToWindow(this);
        };

        _themeManager.ThemeChanged += (_, _) =>
        {
            _themeManager.ApplyThemeToWindow(this);
        };
    }

    private async void OnRequestTenantSwitch()
    {
        var mediator = _serviceProvider.GetRequiredService<ISender>();
        var currentTenants = await mediator.Send(new GetTenantsForUserQuery(Environment.UserName));

        var tenantSelectionVm = _serviceProvider.GetRequiredService<TenantSelectionViewModel>();
        tenantSelectionVm.LoadTenants(currentTenants);

        var tenantDialog = new TenantSelectionView(tenantSelectionVm)
        {
            Owner = this
        };

        tenantSelectionVm.TenantConfirmed += _ =>
        {
            tenantDialog.DialogResult = true;
            tenantDialog.Close();
        };

        _ = tenantDialog.ShowDialog();
    }

    private void OnRequestNewInvoiceDialog()
    {
        var createVm = _serviceProvider.GetRequiredService<CreateInvoiceViewModel>();
        var dialog = new CreateInvoiceDialog(createVm)
        {
            Owner = this
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            // Refresh invoices list
            var invoicesVm = _serviceProvider.GetRequiredService<InvoicesListViewModel>();
            _ = invoicesVm.LoadInvoicesAsync();
        }
    }
}
