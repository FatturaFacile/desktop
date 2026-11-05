// <copyright file="ApplicationServiceRegistration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Behaviors;
using FatturaFacile.Application.Strategies;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Application;

/// <summary>
/// Service collection extensions registering Application layer dependencies.
/// </summary>
public static class ApplicationServiceRegistration
{
    /// <summary>
    /// Registers MediatR CQRS handlers, AutoMapper profiles, FluentValidation behaviors, and localized country strategies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly).AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));

        services.AddSingleton<ICountryStrategyFactory, CountryStrategyFactory>();

        return services;
    }
}
