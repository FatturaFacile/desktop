// <copyright file="App.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using AutoMapper;
using FatturaFacile.Application;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Infrastructure;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using FatturaFacile.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FatturaFacile.Wpf;

/// <summary>
/// Interaction logic for App.xaml, bootstrapping Host, Windows Hello security, and ERP modules.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    /// <summary>
    /// Handles application startup by enforcing Windows Hello authentication, initializing DB, and displaying MainWindow.
    /// </summary>
    /// <param name="e">The startup event arguments.</param>
    protected override async void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder(e.Args)
            .ConfigureServices((ctx, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructureServices(ctx.Configuration);
                services.AddSingleton<ILocalizationService>(LocalizationService.Instance);

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<InvoicingViewModel>();
                services.AddSingleton<AccountingViewModel>();
                services.AddSingleton<ContactsViewModel>();
                services.AddSingleton<ProductsViewModel>();
                services.AddSingleton<PayrollViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        // 1. Mandatory Windows Hello / Passkey User Authentication
        var authService = _host.Services.GetRequiredService<IUserAuthenticationService>();
        var isAuthenticated = await authService.AuthenticateCurrentUserAsync("Verifica identità biometrica per accedere a FatturaFacile ERP Enterprise.");

        if (!isAuthenticated)
        {
            MessageBox.Show(
                "Autenticazione biometrica / PIN Windows Hello non riuscita o annullata. L'applicazione verrà terminata immediatamente.",
                "FatturaFacile ERP - Accesso Negato",
                MessageBoxButton.OK,
                MessageBoxImage.Stop);

            Shutdown(1);
            return;
        }

        // 2. Database schema creation & seeding
        var dbContext = _host.Services.GetRequiredService<FatturaFacileDbContext>();
        await DatabaseSeeder.SeedAsync(dbContext);

        // 3. Strict AutoMapper Configuration Validation
        var mapper = _host.Services.GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();

        // 4. Initialize MainViewModel and Show MainWindow
        var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
        await mainViewModel.InitializeAsync();
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    /// <summary>
    /// Handles application exit by disposing the generic host.
    /// </summary>
    /// <param name="e">The exit event arguments.</param>
    protected override async void OnExit(ExitEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
