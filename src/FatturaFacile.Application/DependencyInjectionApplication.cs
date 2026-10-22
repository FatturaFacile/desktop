// <copyright file="DependencyInjectionApplication.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Application;

/// <summary>
/// Provides dependency injection configuration for the Application layer,
/// including AutoMapper, FluentValidation, MediatR handlers, and pipeline behaviours.
/// </summary>
public static class DependencyInjectionApplication
{
    /// <summary>
    /// Registers all Application layer services and cross-cutting concerns
    /// required by the application.
    /// </summary>
    /// <param name="services">
    /// The service collection used to register application services.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance for chaining.
    /// </returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // 1. Register AutoMapper
        _ = services.AddAutoMapper(cfg => cfg.AddMaps(typeof(AssemblyReference).Assembly));

        // 2. Register FluentValidation
        _ = services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);

        // 3. Register MediatR with pipeline behaviors in strict order:
        // Logging -> TenantContext -> Validation -> Transaction
        _ = services.AddMediatR(cfg =>
        {
            _ = cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
            _ = cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            _ = cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));
            _ = cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            _ = cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        return services;
    }
}
