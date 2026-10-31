// <copyright file="DependencyInjectionApplication.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
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
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
		
		
		
		 var assembly = typeof(ApplicationAssemblyMarker).Assembly;

        // AutoMapper
        _ = services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(assembly);
        });

        // MediatR & Pipeline Behaviors
        _ = services.AddMediatR(cfg =>
        {
            _ = cfg.RegisterServicesFromAssembly(assembly);
            _ = cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        _ = services.AddValidatorsFromAssembly(assembly);

        // CQRS Dispatcher Abstraction Facade
        _ = services.AddSingleton<IDispatcher, Dispatcher>();

        return services;
    }
}
