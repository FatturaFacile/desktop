using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FatturaFacile.Application.Shared.Context;
using FatturaFacile.Extensions;
using FatturaFacile.Infrastructure.Common.Configuration;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Infrastructure.Persistence;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using FatturaFacile.Accounting.Infrastructure.Persistence;
using FatturaFacile.Crm.Infrastructure.Persistence;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Desktop.Wpf.ViewModels;
using FatturaFacile.Identity.Application.Security;

namespace FatturaFacile.Desktop.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Establish offline SQLite local application data storage path
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FatturaFacile"
        );
        Directory.CreateDirectory(appDataFolder);

        var sqliteDbPath = Path.Combine(appDataFolder, "data.sqlite");
        var connectionString = $"Data Source={sqliteDbPath}";

        // 2. Build in-process generic Host
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register desktop stateful contexts as Singletons to bridge background MediatR handlers
                var tenantContext = new DesktopTenantContext();
                var userContext = new DesktopUserContext();
                var localizer = new DesktopLocalizer();

                services.AddSingleton<DesktopTenantContext>(tenantContext);
                services.AddSingleton<ITenantContext>(tenantContext);

                services.AddSingleton<DesktopUserContext>(userContext);
                services.AddSingleton<ICurrentUserContext>(userContext);

                services.AddSingleton<IDesktopLocalizer>(localizer);
                services.AddSingleton<INavigationService, NavigationService>();

                // Configure FatturaFacile ERP engine (modules, Argon2id, EF Core persistence)
                services.AddFatturaFacile(options =>
                {
                    options.UseDatabase(DatabaseProvider.Sqlite, connectionString);
                    options.ConfigureLocalization(
                        supportedCultures: ["en", "it", "de", "fr"],
                        defaultCulture: "en"
                    );
                });

                // Override scoped context registrations with desktop singletons
                services.AddScoped<ITenantContext>(_ => tenantContext);
                services.AddScoped<ICurrentUserContext>(_ => userContext);

                // ViewModels
                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<LoginViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddTransient<InvoiceListViewModel>();
                services.AddTransient<InvoiceDetailViewModel>();

                // Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        // 3. Migrate and seed offline SQLite database on initial launch
        await InitializeDatabaseAsync(_host.Services);

        // 4. Resolve and display MainWindow with Login view active
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var navService = _host.Services.GetRequiredService<INavigationService>();
        var shellVm = _host.Services.GetRequiredService<ShellViewModel>();
        var loginVm = _host.Services.GetRequiredService<LoginViewModel>();

        loginVm.LoginSucceeded += () =>
        {
            shellVm.OnAuthenticated(loginVm.TenantSlug, loginVm.Email);
        };

        navService.NavigateTo(loginVm);
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var invoicingDb = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        var accountingDb = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
        var crmDb = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await invoicingDb.Database.EnsureCreatedAsync();
        await accountingDb.Database.EnsureCreatedAsync();
        await crmDb.Database.EnsureCreatedAsync();
        await identityDb.Database.EnsureCreatedAsync();

        // Seed default tenant and administrator user if not existing
        var tenant = await identityDb.Tenants.FirstOrDefaultAsync(t => t.Slug == "default-corp");
        if (tenant == null)
        {
            tenant = Tenant.Create("Default Enterprise AG", "default-corp");
            await identityDb.Tenants.AddAsync(tenant);
            await identityDb.SaveChangesAsync();

            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var protector = scope.ServiceProvider.GetRequiredService<IDataProtectorService>();

            var email = "admin@fatturafacile.ch";
            var normalizedEmail = protector.ComputeBlindIndex(email);
            var encryptedEmail = protector.Encrypt(email);
            var passwordHash = hasher.HashPassword("Admin123$!");

            var user = User.Create(
                tenant.Id,
                normalizedEmail,
                encryptedEmail,
                passwordHash,
                "en"
            );

            await identityDb.Users.AddAsync(user);
            await identityDb.SaveChangesAsync();
        }
    }
}
