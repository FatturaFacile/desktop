// <copyright file="InfrastructureServiceRegistration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Security.Iam;
using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Application.Dss;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Application.InventoryCounting;
using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Application.Mes;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Application.ProjectBilling;
using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Application.Projects;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Application.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Accounting;
using FatturaFacile.Infrastructure.AiCopilot;
using FatturaFacile.Infrastructure.AnalyticalAccounting;
using FatturaFacile.Infrastructure.Copilot;
using FatturaFacile.Infrastructure.CustomerLifecycle;
using FatturaFacile.Infrastructure.Dss;
using FatturaFacile.Infrastructure.FieldService;
using FatturaFacile.Infrastructure.Gelato;
using FatturaFacile.Infrastructure.Helpdesk;
using FatturaFacile.Infrastructure.InventoryCounting;
using FatturaFacile.Infrastructure.Manufacturing;
using FatturaFacile.Infrastructure.Mes;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Procurement;
using FatturaFacile.Infrastructure.ProjectBilling;
using FatturaFacile.Infrastructure.ProjectEvm;
using FatturaFacile.Infrastructure.Projects;
using FatturaFacile.Infrastructure.Receivables;
using FatturaFacile.Infrastructure.Reporting;
using FatturaFacile.Infrastructure.Repositories;
using FatturaFacile.Infrastructure.ResourcePlanning;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using FatturaFacile.Infrastructure.SupplierRebates;
using FatturaFacile.Infrastructure.Sync;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FatturaFacile.Infrastructure;

/// <summary>
/// Service collection extension registering persistence, authentication, and document export infrastructure.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Registers EF Core SQLite database context, repositories, unit of work, and native infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FatturaFacile");
        _ = Directory.CreateDirectory(dataDirectory);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? $"Data Source={Path.Combine(dataDirectory, "fattura-facile.sqlite")}";

        string databasePath;

        using (SqliteConnection probe = new(connectionString))
        {
            databasePath = Path.GetFullPath(probe.DataSource);
        }

        _ = services.AddSingleton<ICurrentTenantService, CurrentTenantService>();
        _ = services.AddSingleton<TenantSaveChangesInterceptor>();

        var keyPath = configuration["Security:FieldEncryptionKeyPath"] ?? DpapiKeyStore.DefaultPath;
        _ = services.AddSingleton(_ => new AesGcmFieldEncryptor(DpapiKeyStore.LoadOrCreate(keyPath)));
        _ = services.AddSingleton(new DatabasePaths(databasePath, keyPath));
        _ = services.AddSingleton<IBackupService, ZipBackupService>();
        _ = services.AddSingleton<IReportTemplateStore, FileSystemReportTemplateStore>();
        _ = services.AddSingleton<IReportRenderer, DocIoReportRenderer>();
        _ = services.AddSingleton<IDocumentArchive, FileSystemDocumentArchive>();
        _ = services.AddScoped<IWithholdingTaxTariffStore, WithholdingTaxTariffStore>();
        _ = services.AddScoped<IDataRetentionService, DataRetentionService>();
        _ = services.AddSingleton<ISignedDocumentReader, CmsSignedDocumentReader>();
        _ = services.AddSingleton(sp => CreateSdiTransmitter(configuration, sp.GetRequiredService<DatabasePaths>()));

        _ = services.AddDbContext<FatturaFacileDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<TenantSaveChangesInterceptor>();
            var syncInterceptor = sp.GetRequiredService<SyncChangeCaptureInterceptor>();
            _ = options.UseSqlite(connectionString)
                   .AddInterceptors(interceptor, syncInterceptor);
        });

        AddSync(services, configuration);
        AddManufacturing(services, databasePath);

        _ = services.AddScoped<IDssSnapshotStore, DssSnapshotStore>();
        _ = services.AddScoped<IDssApiClient, DssApiClient>();
        _ = services.AddScoped<ICustomerLifecycleApiClient, CustomerLifecycleApiClient>();
        _ = services.AddScoped<ICustomerActivityOutbox, CustomerActivityOutbox>();
        _ = services.AddScoped<IProjectEvmSnapshotStore, ProjectEvmSnapshotStore>();
        _ = services.AddScoped<IProjectEvmApiClient, ProjectEvmApiClient>();
        _ = services.AddScoped<IProjectBillingSnapshotStore, ProjectBillingSnapshotStore>();
        _ = services.AddScoped<IProjectBillingApiClient, ProjectBillingApiClient>();
        _ = services.AddScoped<ISupplierRebatesCache, SupplierRebatesCache>();
        _ = services.AddScoped<ISupplierRebatesApiClient, SupplierRebatesApiClient>();
        _ = services.AddScoped<IAnalyticalSnapshotStore, AnalyticalSnapshotStore>();
        _ = services.AddScoped<IAnalyticalOperationQueue, AnalyticalOperationQueue>();
        _ = services.AddScoped<IAnalyticalApiClient, AnalyticalApiClient>();
        _ = services.AddScoped<ILedgerApiClient, LedgerApiClient>();
        _ = services.AddScoped<IFieldServiceStore, FieldServiceStore>();
        _ = services.AddScoped<IFieldServiceApiClient, FieldServiceApiClient>();
        _ = services.AddScoped<IServiceReportPdfSaver, ServiceReportPdfSaver>();
        _ = services.AddScoped<IManufacturingStore, ManufacturingStore>();
        _ = services.AddScoped<IManufacturingApiClient, ManufacturingApiClient>();
        _ = services.AddScoped<IMesStore, MesStore>();
        _ = services.AddScoped<IHelpdeskStore, HelpdeskStore>();
        _ = services.AddScoped<IHelpdeskApiClient, HelpdeskApiClient>();
        _ = services.AddScoped<IResourcePlanningApiClient, ResourcePlanningApiClient>();
        _ = services.AddScoped<IProjectPortfolioStore, ProjectPortfolioStore>();
        _ = services.AddScoped<IProjectApiClient, ProjectApiClient>();
        _ = services.AddScoped<IReceivablesStore, ReceivablesStore>();
        _ = services.AddScoped<IReceivablesApiClient, ReceivablesApiClient>();
        _ = services.AddScoped<IReceivablesFileSaver, ReceivablesFileSaver>();
        _ = services.AddScoped<IProcurementStore, ProcurementStore>();
        _ = services.AddScoped<IProcurementApiClient, ProcurementApiClient>();
        _ = services.AddScoped<IMesApiClient, MesApiClient>();
        AddSecurity(services, configuration, dataDirectory);
        _ = services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        _ = services.AddScoped<IUnitOfWork, UnitOfWork>();

        _ = services.AddSingleton<IUserAuthenticationService, WindowsHelloAuthenticationService>();
        _ = services.AddSingleton<IDocumentExportService, SyncfusionDocumentExportService>();
        _ = services.AddSingleton<IQrCodeEncoder, QrCoderQrCodeEncoder>();
        _ = services.AddSingleton<ICurrentUserService, CurrentUserService>();
        _ = services.AddSingleton<IAuthorizationService, AuthorizationService>();
        _ = services.AddSingleton<IPermissionCache>(sp => new SqlitePermissionCache(connectionString, sp.GetRequiredService<AesGcmFieldEncryptor>()));
        _ = services.AddSingleton<IApiSession, ApiSession>();
        _ = services.AddTransient<ApiSessionHandler>();
        _ = services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        CopilotOptions copilot = new() { BaseUrl = configuration["Copilot:BaseUrl"] ?? string.Empty, AccessToken = configuration["Copilot:AccessToken"] ?? string.Empty };
        _ = services.AddSingleton(copilot);
        _ = services.AddHttpClient<ICopilotApiClient, CopilotApiClient>(client =>
        {
            client.BaseAddress = copilot.GetBaseAddress();
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        _ = services.AddHttpClient<IGelatoApiClient, GelatoApiClient>(client =>
        {
            client.BaseAddress = copilot.GetBaseAddress();
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    /// <summary>
    /// Chooses the SDI channel: an HTTP intermediary when "Sdi:Channel" is "Http" and "Sdi:BaseUrl" is set, otherwise the SDI folders.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="paths">The data folder.</param>
    /// <returns>The SDI channel.</returns>
    private static ISdiTransmitter CreateSdiTransmitter(IConfiguration configuration, DatabasePaths paths) =>
        string.Equals(configuration["Sdi:Channel"], "Http", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(configuration["Sdi:BaseUrl"], UriKind.Absolute, out var address)
            ? new HttpSdiTransmitter(address, configuration["Sdi:ApiKey"])
            : new FolderSdiTransmitter(paths);

    private static void AddSecurity(IServiceCollection services, IConfiguration configuration, string dataDirectory)
    {
        var deviceKeyPath = configuration["Security:AuditDeviceKeyPath"] ?? Path.Combine(dataDirectory, "audit-device.key");

        _ = services.AddSingleton<IKeyProtector, DpapiKeyProtector>();
        _ = services.AddSingleton<IAuditDeviceKeyProvider>(sp => new AuditDeviceKeyProvider(sp.GetRequiredService<IKeyProtector>(), deviceKeyPath));
        _ = services.AddSingleton<ILocalEncryptionStatusService, LocalEncryptionStatusService>();
        _ = services.AddSingleton<ISecuritySettingsStore>(_ => new JsonSecuritySettingsStore(Path.Combine(dataDirectory, "security-settings.json")));
        _ = services.AddScoped<ILocalAuditQueue, LocalAuditQueue>();

        // The security calls ride on the same typed HTTP client (session, token refresh) as the sync.
        _ = services.AddTransient<ISecurityApiClient>(sp => (SyncApiClient)sp.GetRequiredService<ISyncApiClient>());
    }

    private static void AddManufacturing(IServiceCollection services, string databasePath)
    {
        // The workbench keeps its own SQLite file next to the ERP database: the offline store is rebuildable from the server and migrates independently.
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = Path.Combine(Path.GetDirectoryName(databasePath) ?? ".", "fattura-facile-manufacturing.sqlite"),
        };

        _ = services.AddDbContextFactory<ManufacturingDbContext>(options => options.UseSqlite(builder.ConnectionString));
        _ = services.AddSingleton<ManufacturingDatabaseInitializer>();
        _ = services.AddSingleton<WorkbenchStore>();
        _ = services.AddSingleton<IManufacturingReadStore>(sp => sp.GetRequiredService<WorkbenchStore>());
        _ = services.AddSingleton<IManufacturingOutboxStore>(sp => sp.GetRequiredService<WorkbenchStore>());
        _ = services.AddSingleton<IManufacturingSnapshotStore>(sp => sp.GetRequiredService<WorkbenchStore>());
        _ = services.AddSingleton<IManufacturingApiGateway>(sp => new ManufacturingApiGateway(
            (SyncApiClient)sp.GetRequiredService<ISyncApiClient>(),
            sp.GetRequiredService<TimeProvider>()));
    }

    private static void AddSync(IServiceCollection services, IConfiguration configuration)
    {
        _ = services.Configure<SyncOptions>(configuration.GetSection(SyncOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);
        _ = services.AddSingleton<SyncSession>();
        _ = services.AddSingleton<SyncChangeCaptureInterceptor>();
        _ = services.AddSingleton<SyncCoordinator>();
        _ = services.AddHostedService<SyncBackgroundService>();
        _ = services.AddScoped<ISyncStore, SyncStore>();

        _ = services.AddHttpClient<ISyncApiClient, SyncApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SyncOptions>>().Value;
            if (options.IsEnabled)
            {
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd(SyncOptions.UrlSeparator) + SyncOptions.UrlSeparator);
            }

            client.Timeout = TimeSpan.FromSeconds(30);
        });

        _ = services.AddSingleton<AiCopilotCoordinator>();
        _ = services.AddSingleton<IAiCopilot>(sp => sp.GetRequiredService<AiCopilotCoordinator>());
        _ = services.AddScoped<IAiCopilotStore, AiCopilotStore>();
        _ = services.AddScoped<IAiApiClient, AiApiClient>();
        _ = services.AddScoped<IInventoryCountingApiClient, InventoryCountingApiClient>();
        _ = services.AddScoped<AiCopilotService>();
        _ = services.AddScoped<ISyncEntityHandler>(sp => new EfSyncEntityHandler<Contact>(
            SyncCatalog.Contacts,
            SyncCatalog.Contacts,
            sp.GetRequiredService<FatturaFacileDbContext>(),
            sp.GetRequiredService<ICurrentTenantService>(),
            sp.GetRequiredService<TimeProvider>()));

        // Products are pulled only: the write route of the catalog is not mapped yet.
        _ = services.AddScoped<ISyncEntityHandler>(sp => new EfSyncEntityHandler<Product>(
            SyncCatalog.Products,
            writeRoute: null,
            sp.GetRequiredService<FatturaFacileDbContext>(),
            sp.GetRequiredService<ICurrentTenantService>(),
            sp.GetRequiredService<TimeProvider>()));
    }
}
