using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FatturaFacile.Application.Shared.Behaviors;
using FatturaFacile.Application.Shared.Context;
using FatturaFacile.Application.Shared.Localization;
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
using FatturaFacile.Crm.Domain.Repositories;
using FatturaFacile.Crm.Infrastructure.Persistence;
using FatturaFacile.Crm.Infrastructure.Repositories;
using FatturaFacile.Accounting.Domain.Repositories;
using FatturaFacile.Accounting.Infrastructure.Persistence;
using FatturaFacile.Accounting.Infrastructure.Repositories;
using FatturaFacile.Identity.Application.Commands;
using FatturaFacile.Identity.Application.Security;
using FatturaFacile.Identity.Application.Validators;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Persistence;
using FatturaFacile.Identity.Infrastructure.Repositories;

namespace FatturaFacile.Extensions;

/// <summary>
/// Fluent ServiceCollection extensions to configure and bootstrap the FatturaFacile ERP engine.
/// </summary>
public static class FatturaFacileServiceCollectionExtensions
{
    /// <summary>
    /// Registers all FatturaFacile bounded contexts, multi-tenant persistence, localization, and identity engine.
    /// </summary>
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
        services.AddScoped<AuditAndSoftDeleteInterceptor>();
        services.AddScoped<TenantSaveChangesInterceptor>();
        services.AddScoped<OutboxDomainEventsInterceptor>();

        // MediatR & Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(CreateInvoiceCommand).Assembly,
                typeof(LoginUserCommand).Assembly,
                typeof(FatturaFacileServiceCollectionExtensions).Assembly
            );
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateInvoiceCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginUserCommandValidator>();

        // AutoMapper
        services.AddAutoMapper(typeof(InvoicingMappingProfile).Assembly);

        // Identity Security Services
        services.TryAddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
        services.TryAddSingleton<IDataProtectorService, DataProtectorService>();
        services.TryAddSingleton<ITokenService, TokenService>();
        services.TryAddSingleton(new JwtOptions());

        // Multi-Provider Persistence
        services.AddFatturaFacilePersistence(options);

        // Repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IGeneralLedgerRepository, GeneralLedgerRepository>();

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ISecurityAuditLogRepository, SecurityAuditLogRepository>();

        return services;
    }

    /// <summary>
    /// Adds persistence contexts configured with runtime provider switching.
    /// </summary>
    public static IServiceCollection AddFatturaFacilePersistence(
        this IServiceCollection services,
        FatturaFacileOptions options)
    {
        // Invoicing DbContext
        services.AddDbContext<InvoicingDbContext>((sp, dbOpts) =>
        {
            dbOpts.ConfigureDatabaseProvider(options.Provider, options.ConnectionString);
            dbOpts.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxDomainEventsInterceptor>());
        });

        // CRM DbContext
        services.AddDbContext<CrmDbContext>((sp, dbOpts) =>
        {
            dbOpts.ConfigureDatabaseProvider(options.Provider, options.ConnectionString);
            dbOpts.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>());
        });

        // Accounting DbContext
        services.AddDbContext<AccountingDbContext>((sp, dbOpts) =>
        {
            dbOpts.ConfigureDatabaseProvider(options.Provider, options.ConnectionString);
            dbOpts.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>());
        });

        // Identity DbContext
        services.AddDbContext<IdentityDbContext>((sp, dbOpts) =>
        {
            dbOpts.ConfigureDatabaseProvider(options.Provider, options.ConnectionString);
            dbOpts.AddInterceptors(
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>());
        });

        return services;
    }
}
