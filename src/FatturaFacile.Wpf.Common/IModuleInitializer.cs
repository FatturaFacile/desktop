// <copyright file="IModuleInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contract for UI modules to register their views, view models, and services in dependency injection.
/// </summary>
public interface IModuleInitializer
{

    /// <summary>
    /// Configures services for this module.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration.</param>
    void Initialize(
        IServiceCollection services,
        IConfiguration     configuration);

}
