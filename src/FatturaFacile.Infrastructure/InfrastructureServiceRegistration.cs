// <copyright file="InfrastructureServiceRegistration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Repositories;
using FatturaFacile.Infrastructure.Services;
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

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=fattura-facile.sqlite";

        _ = services.AddSingleton<ICurrentTenantService, CurrentTenantService>();
        _ = services.AddSingleton<TenantSaveChangesInterceptor>();

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

        return services;
    }
}
