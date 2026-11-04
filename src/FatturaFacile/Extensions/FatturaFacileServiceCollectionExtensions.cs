// <copyright file="FatturaFacileServiceCollectionExtensions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Accounting.Domain.Repositories;
using FatturaFacile.Accounting.Infrastructure.Persistence;
using FatturaFacile.Accounting.Infrastructure.Repositories;
using FatturaFacile.Application.Shared.Behaviors;
using FatturaFacile.Application.Shared.Context;
using FatturaFacile.Application.Shared.Localization;
using FatturaFacile.Crm.Domain.Repositories;
using FatturaFacile.Crm.Infrastructure.Persistence;
using FatturaFacile.Crm.Infrastructure.Repositories;
using FatturaFacile.Identity.Application.Commands;
using FatturaFacile.Identity.Application.Security;
using FatturaFacile.Identity.Application.Validators;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Persistence;
using FatturaFacile.Identity.Infrastructure.Repositories;
using FatturaFacile.Infrastructure.Common.Configuration;
using FatturaFacile.Infrastructure.Common.Extensions;
using FatturaFacile.Infrastructure.Common.Interceptors;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Application.Mappings;
using FatturaFacile.Invoicing.Application.Validators;
using FatturaFacile.Invoicing.Domain.Repositories;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using FatturaFacile.Invoicing.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FatturaFacile.Extensions;

/// <summary>
/// Fluent ServiceCollection extensions to configure and bootstrap the FatturaFacile ERP engine.
/// </summary>
public static class FatturaFacileServiceCollectionExtensions
{
    /// <summary>
    /// Registers all FatturaFacile bounded contexts, multi-tenant persistence, localization, and identity engine.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    /// <param name="configureOptions">An optional delegate to configure <see cref="FatturaFacileOptions"/>.</param>
    /// <returns>The service collection for fluent chaining.</returns>
    public static IServiceCollection AddFatturaFacile(
        this IServiceCollection services,
        Action<FatturaFacileOptions>? configureOptions = null)
    {
        var options = new FatturaFacileOptions();
        configureOptions?.Invoke(options);

        // Core Tenancy & User Context
        services.TryAddScoped<ITenantContext, TenantContext>();
        services.TryAddScoped<ICurrentUserContext, CurrentUserContext>();

        // Localization Engine
        services.TryAddSingleton<IErrorLocalizer, ErrorLocalizer>();

        // Interceptors
        _ = services.AddScoped<AuditAndSoftDeleteInterceptor>();
        _ = services.AddScoped<TenantSaveChangesInterceptor>();
        _ = services.AddScoped<OutboxDomainEventsInterceptor>();

        // MediatR & Pipeline Behaviors
        _ = services.AddMediatR(cfg =>
        {
            _ = cfg.RegisterServicesFromAssemblies(
                typeof(CreateInvoiceCommand).Assembly,
                typeof(LoginUserCommand).Assembly,
                typeof(FatturaFacileServiceCollectionExtensions).Assembly);
            _ = cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            _ = cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        _ = services.AddValidatorsFromAssemblyContaining<CreateInvoiceCommandValidator>();
        _ = services.AddValidatorsFromAssemblyContaining<LoginUserCommandValidator>();

        // AutoMapper
        services.AddAutoMapper(typeof(InvoicingMappingProfile).Assembly);

        // Identity Security Services
        services.TryAddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
        services.TryAddSingleton<IDataProtectorService, DataProtectorService>();
        services.TryAddSingleton<ITokenService, TokenService>();
        services.TryAddSingleton(new JwtOptions());

        // Multi-Provider Persistence
        _ = services.AddFatturaFacilePersistence(options);

        // Repositories
        _ = services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        _ = services.AddScoped<IContactRepository, ContactRepository>();
        _ = services.AddScoped<IGeneralLedgerRepository, GeneralLedgerRepository>();

        _ = services.AddScoped<ITenantRepository, TenantRepository>();
        _ = services.AddScoped<IUserRepository, UserRepository>();
        _ = services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        _ = services.AddScoped<ISecurityAuditLogRepository, SecurityAuditLogRepository>();

        return services;
    }

    /// <summary>
    /// Adds persistence contexts configured with runtime provider switching.
    /// </summary>
    /// <param name="services">The service collection to register contexts into.</param>
    /// <param name="options">The configured FatturaFacile engine options.</param>
    /// <returns>The service collection for fluent chaining.</returns>
    public static IServiceCollection AddFatturaFacilePersistence(
        this IServiceCollection services,
        FatturaFacileOptions options)
    {
        // Invoicing DbContext
        _ = services.AddDbContext<InvoicingDbContext>((sp, dbOpts) =>
        {
            _ = dbOpts.ConfigureDatabaseProvider(options.Provider, options.ConnectionString);
            _ = dbOpts.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxDomainEventsInterceptor>());
        });

        // CRM DbContext
        _ = services.AddDbContext<CrmDbContext>((sp, dbOpts) =>
        {
            _ = dbOpts.ConfigureDatabaseProvider(options.Provider, options.ConnectionString);
            _ = dbOpts.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>());
        });

        // Accounting DbContext
        _ = services.AddDbContext<AccountingDbContext>((sp, dbOpts) =>
        {
            _ = dbOpts.ConfigureDatabaseProvider(options.Provider, options.ConnectionString);
            _ = dbOpts.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>());
        });

        // Identity DbContext
        _ = services.AddDbContext<IdentityDbContext>((sp, dbOpts) =>
        {
            _ = dbOpts.ConfigureDatabaseProvider(options.Provider, options.ConnectionString);
            _ = dbOpts.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>());
        });

        return services;
    }
}
