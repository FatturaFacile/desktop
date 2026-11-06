// <copyright file="InfrastructureServiceRegistration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Reporting;
using FatturaFacile.Infrastructure.Repositories;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        _ = services.AddDbContext<FatturaFacileDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<TenantSaveChangesInterceptor>();
            _ = options.UseSqlite(connectionString)
                   .AddInterceptors(interceptor);
        });

        _ = services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        _ = services.AddScoped<IUnitOfWork, UnitOfWork>();

        _ = services.AddSingleton<IUserAuthenticationService, WindowsHelloAuthenticationService>();
        _ = services.AddSingleton<IDocumentExportService, SyncfusionDocumentExportService>();
        _ = services.AddSingleton<IQrCodeEncoder, QrCoderQrCodeEncoder>();
        _ = services.AddSingleton<ICurrentUserService, CurrentUserService>();
        _ = services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}
