// <copyright file="App.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using FatturaFacile.Application;
using FatturaFacile.Application.Ports;
using FatturaFacile.Infrastructure;
using FatturaFacile.Wpf.Licensing;
using FatturaFacile.Wpf.Navigation;
using FatturaFacile.Wpf.Theming;
using FatturaFacile.Wpf.ViewModels;
using FatturaFacile.Wpf.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FatturaFacile.Wpf;

/// <summary>
/// Interaction logic for App.xaml and top-level host startup configuration.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    /// <summary>
    /// Configures the generic host and runs application startup sequences.
    /// </summary>
    /// <param name="e">Event data associated with application startup.</param>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                _ = config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Clean Architecture Layers
                _ = services.AddApplication();
                _ = services.AddInfrastructure();

                // AutoMapper Presentation Profiles
                _ = services.AddAutoMapper(cfg =>
                {
                    cfg.AddMaps(typeof(InvoiceViewModelProfile).Assembly);
                });

                // Theming & Licensing
                _ = services.AddSingleton<ThemeManagerService>();
                _ = services.AddSingleton<IThemeManagerService>(sp => sp.GetRequiredService<ThemeManagerService>());

                // Navigation Service
                _ = services.AddSingleton<INavigationService, NavigationService>();

                // ViewModels
                _ = services.AddTransient<LoginViewModel>();
                _ = services.AddTransient<TenantSelectionViewModel>();
                _ = services.AddSingleton<MainShellViewModel>();
                _ = services.AddSingleton<DashboardViewModel>();
                _ = services.AddSingleton<InvoicesListViewModel>();
                _ = services.AddSingleton<CustomersViewModel>();
                _ = services.AddSingleton<ProductsViewModel>();
                _ = services.AddSingleton<BackupSettingsViewModel>();
                _ = services.AddSingleton<SettingsViewModel>();
                _ = services.AddTransient<CreateInvoiceViewModel>();

                // Views
                _ = services.AddTransient<LoginView>();
                _ = services.AddTransient<TenantSelectionView>();
                _ = services.AddSingleton<MainShellView>();
            })
            .Build();

        await _host.StartAsync();

        // Register Syncfusion Community License dynamically from config
        var config = _host.Services.GetRequiredService<IConfiguration>();
        SyncfusionLicenseBootstrapper.RegisterLicense(config);

        // Run Idempotent First-Run Database Seeder
        using (var scope = _host.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            await seeder.SeedAsync();

            // Run silent background backup on startup
            var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
            _ = Task.Run(() => backupService.PerformAutomatedSilentBackupAsync());
        }

        // Authentication Workflow: LoginView -> (TenantSelectionView if > 1) -> MainShellView
        var loginVm = _host.Services.GetRequiredService<LoginViewModel>();
        var loginView = new LoginView(loginVm);

        loginVm.SingleTenantAutoSelected += () =>
        {
            loginView.Close();
            ShowMainShell();
        };

        loginVm.MultipleTenantsDetected += (tenants) =>
        {
            loginView.Close();

            var tenantVm = _host.Services.GetRequiredService<TenantSelectionViewModel>();
            tenantVm.LoadTenants(tenants);
            var tenantView = new TenantSelectionView(tenantVm);

            tenantVm.TenantConfirmed += _ =>
            {
                tenantView.Close();
                ShowMainShell();
            };

            tenantView.Show();
        };

        loginView.Show();
    }

    private void ShowMainShell()
    {
        var mainShell = _host!.Services.GetRequiredService<MainShellView>();
        mainShell.Show();
    }

    /// <summary>
    /// Executes clean shutdown sequences and disposes generic host.
    /// </summary>
    /// <param name="e">Event data associated with application exit.</param>
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            try
            {
                using var scope = _host.Services.CreateScope();
                var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                _ = await backupService.PerformAutomatedSilentBackupAsync();
            }
            catch { }

            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
