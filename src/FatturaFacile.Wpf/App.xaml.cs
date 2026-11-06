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
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels;
using FatturaFacile.Wpf.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syncfusion.Licensing;

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

        // Syncfusion community license key: the value in appsettings.json wins; the merged configuration
        // (e.g. the developer environment variable Syncfusion__LicenseKey) is used only when appsettings.json is empty.
        var appSettings = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var licenseKey = appSettings["Syncfusion:LicenseKey"];

        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            licenseKey = _host.Services.GetRequiredService<IConfiguration>()["Syncfusion:LicenseKey"];
        }

        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            SyncfusionLicenseProvider.RegisterLicense(licenseKey);
        }

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
                _ = services.AddSingleton<SwissPayrollViewModel>();
                _ = services.AddSingleton<CompanySettingsViewModel>();
                _ = services.AddSingleton<UsersViewModel>();
                _ = services.AddSingleton<TimeTrackingViewModel>();
                _ = services.AddSingleton<FinanceViewModel>();
                _ = services.AddSingleton(static sp => new ShellModules(
                    sp.GetRequiredService<InvoicingViewModel>(),
                    sp.GetRequiredService<AccountingViewModel>(),
                    sp.GetRequiredService<ContactsViewModel>(),
                    sp.GetRequiredService<ProductsViewModel>(),
                    sp.GetRequiredService<PayrollViewModel>(),
                    sp.GetRequiredService<CompanySettingsViewModel>(),
                    sp.GetRequiredService<UsersViewModel>())
                {
                    TimeTracking = sp.GetRequiredService<TimeTrackingViewModel>(),
                    Finance = sp.GetRequiredService<FinanceViewModel>(),
                });

                // Shell services
                _ = services.AddSingleton<IFileDialogService, FileDialogService>();
                _ = services.AddSingleton<IApplicationRestarter, ApplicationRestarter>();
                _ = services.AddSingleton<IShellLauncher, ShellLauncher>();
                _ = services.AddSingleton<IReportPrintService, ReportPrintService>();

                // Views
                _ = services.AddSingleton<MainWindow>();
            })
            .Build();

    private async Task RunBootstrapAsync(IHost host)
    {
        try
        {
            await host.StartAsync();

            // 1. Database migrations & seeding
            var dbContext = host.Services.GetRequiredService<FatturaFacileDbContext>();
            await DatabaseSeeder.SeedAsync(dbContext);
            host.Services.GetRequiredService<IReportTemplateStore>().EnsureDefaultTemplates();

            // Working time records whose retention period has expired (art. 73 cpv. 2 OLL 1) are deleted at every start.
            _ = await host.Services.GetRequiredService<IDataRetentionService>().ApplyAllTenantsAsync(DateTime.Today);

            // 2. Strict AutoMapper Configuration Validation
            var mapper = host.Services.GetRequiredService<IMapper>();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            // 3. Show the shell: first-run setup or password sign-in and Show MainWindow
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
