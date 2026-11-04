// <copyright file="App.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Windows;
using FatturaFacile.Accounting.Infrastructure.Persistence;
using FatturaFacile.Application.Shared.Context;
using FatturaFacile.Crm.Infrastructure.Persistence;
using FatturaFacile.Desktop.Wpf.Converters;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Desktop.Wpf.ViewModels;
using FatturaFacile.Desktop.Wpf.Views;
using FatturaFacile.Extensions;
using FatturaFacile.Identity.Application.Security;
using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Common.Configuration;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FatturaFacile.Desktop.Wpf;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
internal sealed partial class App : System.Windows.Application, IDisposable
{
    private IHost? _host;

    /// <inheritdoc />
    public void Dispose() => _host?.Dispose();

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Establish offline SQLite local application data storage path
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FatturaFacile");
        _ = Directory.CreateDirectory(appDataFolder);

        var sqliteDbPath = Path.Combine(appDataFolder, "data.sqlite");
        var connectionString = $"Data Source={sqliteDbPath}";

        // 2. Build in-process generic Host
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Register desktop stateful contexts as Singletons to bridge background MediatR handlers
                var tenantContext = new DesktopTenantContext();
                var userContext = new DesktopUserContext();
                var localizer = new DesktopLocalizer();

                _ = services.AddSingleton<DesktopTenantContext>(tenantContext);
                _ = services.AddSingleton<ITenantContext>(tenantContext);

                _ = services.AddSingleton<DesktopUserContext>(userContext);
                _ = services.AddSingleton<ICurrentUserContext>(userContext);

                _ = services.AddSingleton<IDesktopLocalizer>(localizer);
                _ = services.AddSingleton<INavigationService, NavigationService>();

                // Configure FatturaFacile ERP engine (modules, Argon2id, EF Core persistence)
                _ = services.AddFatturaFacile(options =>
                {
                    _ = options.UseDatabase(DatabaseProvider.Sqlite, connectionString);
                    _ = options.ConfigureLocalization(
                        supportedCultures: ["en", "it", "de", "fr"]);
                });

                // Override scoped context registrations with desktop singletons
                _ = services.AddScoped<ITenantContext>(_ => tenantContext);
                _ = services.AddScoped<ICurrentUserContext>(_ => userContext);

                // ViewModels
                _ = services.AddSingleton<ShellViewModel>();
                _ = services.AddSingleton<LoginViewModel>();
                _ = services.AddSingleton<DashboardViewModel>();
                _ = services.AddTransient<InvoiceListViewModel>();
                _ = services.AddTransient<InvoiceDetailViewModel>();

                // Views and Converters (registered to enable DI and satisfy Roslyn analysis)
                _ = services.AddTransient<DashboardView>();
                _ = services.AddTransient<InvoiceDetailView>();
                _ = services.AddTransient<InvoiceListView>();
                _ = services.AddTransient<LoginView>();
                _ = services.AddSingleton<BooleanToVisibilityConverter>();
                _ = services.AddSingleton<SwissCurrencyConverter>();

                // Windows
                _ = services.AddSingleton<MainWindow>();
            })
            .Build();

        _host?.Dispose();
        _host = host;
        host.Start();

        // 3. Migrate and seed offline SQLite database on initial launch
        InitializeDatabase(host.Services);

        // 4. Resolve and display MainWindow with Login view active
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        var navService = host.Services.GetRequiredService<INavigationService>();
        var shellVm = host.Services.GetRequiredService<ShellViewModel>();
        var loginVm = host.Services.GetRequiredService<LoginViewModel>();

        loginVm.LoginSucceeded += (_, _) => shellVm.OnAuthenticated(loginVm.TenantSlug, loginVm.Email);

        navService.NavigateTo(loginVm);
        mainWindow.Show();
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            _host.Dispose();
            _host = null;
        }

        base.OnExit(e);
    }

    private static void InitializeDatabase(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var invoicingDb = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        var accountingDb = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
        var crmDb = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        _ = invoicingDb.Database.EnsureCreated();
        _ = accountingDb.Database.EnsureCreated();
        _ = crmDb.Database.EnsureCreated();
        _ = identityDb.Database.EnsureCreated();

        // Seed default tenant and administrator user if not existing
        var tenant = identityDb.Tenants.FirstOrDefault(t => t.Slug == "default-corp");
        if (tenant == null)
        {
            tenant = Tenant.Create("Default Enterprise AG", "default-corp");
            _ = identityDb.Tenants.Add(tenant);
            _ = identityDb.SaveChanges();

            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var protector = scope.ServiceProvider.GetRequiredService<IDataProtectorService>();

            const string email = "admin@fatturafacile.ch";
            var normalizedEmail = protector.ComputeBlindIndex(email);
            var encryptedEmail = protector.Encrypt(email);
            var passwordHash = hasher.HashPassword("Admin123$!");

            var user = User.Create(
                tenant.Id,
                normalizedEmail,
                encryptedEmail,
                passwordHash);

            _ = identityDb.Users.Add(user);
            _ = identityDb.SaveChanges();
        }
    }
}
