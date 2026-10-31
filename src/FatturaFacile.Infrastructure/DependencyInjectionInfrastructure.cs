// <copyright file="DependencyInjectionInfrastructure.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Repositories;
using FatturaFacile.Infrastructure.Backup;
using FatturaFacile.Infrastructure.CrossBorder;
using FatturaFacile.Infrastructure.Localization;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Persistence.Repositories;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure dependencies, persistence contexts,
/// security providers, cross-border services, and repositories into the dependency injection container.
/// </summary>
public static class DependencyInjectionInfrastructure
{
    /// <summary>
    /// Registers infrastructure services using default application data path and database configuration.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(localAppData, "FatturaFacile");
        _ = Directory.CreateDirectory(appFolder);
        var databasePath = Path.Combine(appFolder, "fatturafacile.db");
        var connectionString = $"Data Source={databasePath}";

        return services.AddInfrastructure(connectionString);
    }

    /// <summary>
    /// Registers infrastructure services, SQLite EF Core database context, unit of work, and repository implementations.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="connectionString">The SQLite connection string targeting the application database.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string                  connectionString)
    {
        // 1. Core Services (Singletons for WPF desktop runtime session lifetime)
        _ = services.AddSingleton<ITenantProvider, TenantProvider>();
        _ = services.AddSingleton<ICurrentUserService, CurrentUserService>();
        _ = services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        _ = services.AddSingleton<ILocalizationService, SqliteLocalizationService>();
        _ = services.AddSingleton<IInvoiceDocumentService, InvoiceDocumentService>();

        _ = services.AddSingleton<CurrentTenantService>();
        _ = services.AddSingleton<Application.Ports.ICurrentTenantService>(sp => sp.GetRequiredService<CurrentTenantService>());

        // 2. EF Core SQLite Contexts
        _ = services.AddDbContext<AppDbContext>((sp, options) =>
        {
            _ = options.UseSqlite(connectionString);
        });

        _ = services.AddDbContext<FatturaFacileDbContext>(optionsAction: options => _ = options.UseSqlite(connectionString: connectionString));

        _ = services.AddScoped<IFatturaFacileDbContext>(implementationFactory: sp => sp.GetRequiredService<FatturaFacileDbContext>());
        _ = services.AddScoped<Domain.Repositories.IUnitOfWork>(implementationFactory: sp => sp.GetRequiredService<FatturaFacileDbContext>());

        // 3. Repositories
        _ = services.AddScoped(
                               serviceType: typeof(Domain.Repositories.IRepository<,>),
                               implementationType: typeof(Repository<,>));

        _ = services.AddScoped<IClientRepository, ClientRepository>();
        _ = services.AddScoped<IItalianInvoiceRepository, ItalianInvoiceRepository>();
        _ = services.AddScoped<ISwissQrBillRepository, SwissQrBillRepository>();
        _ = services.AddScoped<ITireRecordRepository, TireRecordRepository>();
        _ = services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        _ = services.AddScoped<IUserRepository, UserRepository>();
        _ = services.AddScoped<Domain.Repositories.ITenantRepository, Persistence.Repositories.TenantRepository>();
        _ = services.AddScoped<ITranslationRepository, TranslationRepository>();

        // Ports Repositories
        _ = services.AddScoped<Application.Ports.ITenantRepository, Infrastructure.Persistence.TenantRepository>();
        _ = services.AddScoped<Application.Ports.IInvoiceRepository, Infrastructure.Persistence.InvoiceRepository>();
        _ = services.AddScoped<Application.Ports.IContactRepository, Infrastructure.Persistence.ContactRepository>();
        _ = services.AddScoped<Application.Ports.IProductRepository, Infrastructure.Persistence.ProductRepository>();
        _ = services.AddScoped<Application.Ports.IVatCodeRepository, Infrastructure.Persistence.VatCodeRepository>();
        _ = services.AddScoped<Application.Ports.IUnitOfWork, Infrastructure.Persistence.UnitOfWork>();

        // Security & Backup & Seeder
        _ = services.AddTransient<Application.Ports.IAuthenticationService, WindowsAuthenticationService>();
        _ = services.AddScoped<Application.Ports.IBackupService, SqliteBackupService>();
        _ = services.AddScoped<Application.Ports.IDatabaseSeeder, DatabaseSeeder>();

        return services;
    }

    /// <summary>
    /// Registers infrastructure services using the specified configuration
    /// and database connection string.
    /// </summary>
    /// <param name="services">
    /// The service collection used to register application services.
    /// </param>
    /// <param name="configuration">
    /// The application configuration.
    /// </param>
    /// <param name="connectionString">
    /// The database connection string.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance.
    /// </returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration          configuration,
        string                  connectionString) =>
        services.AddInfrastructure(connectionString: connectionString);
}
