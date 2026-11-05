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
internal sealed partial class App : System.Windows.Application, IDisposable
{
    private IHost? _host;

    /// <summary>
    /// Releases the generic host and the services it owns.
    /// </summary>
    public void Dispose()
    {
        _host?.Dispose();
        _host = null;
    }

    /// <summary>
    /// Handles application startup by building the host and launching the asynchronous bootstrap sequence.
    /// </summary>
    /// <param name="e">The startup event arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnStartup(e);

        _host?.Dispose();
        _host = BuildHost(e.Args);

        _ = RunBootstrapAsync(_host);
    }

    /// <summary>
    /// Handles application exit by disposing the generic host.
    /// </summary>
    /// <param name="e">The exit event arguments.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        Dispose();
        base.OnExit(e);
    }

    private static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                _ = services.AddApplicationServices();
                _ = services.AddInfrastructureServices(ctx.Configuration);
                _ = services.AddSingleton<ILocalizationService>(LocalizationService.Instance);

                // ViewModels
                _ = services.AddSingleton<MainViewModel>();
                _ = services.AddSingleton<InvoicingViewModel>();
                _ = services.AddSingleton<AccountingViewModel>();
                _ = services.AddSingleton<ContactsViewModel>();
                _ = services.AddSingleton<ProductsViewModel>();
                _ = services.AddSingleton<PayrollViewModel>();

                // Views
                _ = services.AddSingleton<MainWindow>();
            })
            .Build();

    private async Task RunBootstrapAsync(IHost host)
    {
        try
        {
            await host.StartAsync();

            // 1. Mandatory Windows Hello / Passkey User Authentication
            var authService = host.Services.GetRequiredService<IUserAuthenticationService>();
            var isAuthenticated = await authService.AuthenticateCurrentUserAsync("Verifica identità biometrica per accedere a FatturaFacile ERP Enterprise.");

            if (!isAuthenticated)
            {
                _ = MessageBox.Show(
                    "Autenticazione biometrica / PIN Windows Hello non riuscita o annullata. L'applicazione verrà terminata immediatamente.",
                    "FatturaFacile ERP - Accesso Negato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);

                Shutdown(1);
                return;
            }

            // 2. Database schema creation & seeding
            var dbContext = host.Services.GetRequiredService<FatturaFacileDbContext>();
            await DatabaseSeeder.SeedAsync(dbContext);

            // 3. Strict AutoMapper Configuration Validation
            var mapper = host.Services.GetRequiredService<IMapper>();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            // 4. Initialize MainViewModel and Show MainWindow
            var mainViewModel = host.Services.GetRequiredService<MainViewModel>();
            await mainViewModel.InitializeAsync();
            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            // Bootstrap runs detached from the dispatcher, so failures must be surfaced here.
            _ = MessageBox.Show(
                $"Avvio di FatturaFacile non riuscito: {ex.Message}",
                "FatturaFacile ERP - Errore di Avvio",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }
}
