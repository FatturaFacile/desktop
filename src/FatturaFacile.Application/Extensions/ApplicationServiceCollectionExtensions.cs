// <copyright file="ApplicationServiceCollectionExtensions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Application.Extensions;

/// <summary>
/// Service collection extensions for registering Application layer services.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers Application layer services into the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services) => DependencyInjectionApplication.AddApplication(services);

    /// <summary>
    /// Registers Application layer services into the service collection (alias for AddApplication).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services) => DependencyInjectionApplication.AddApplication(services);
}
