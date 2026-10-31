// <copyright file="BusinessModulesInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Modules;

/// <summary>
/// Registers core business modules, views, and viewmodels in dependency injection.
/// </summary>
public sealed class BusinessModulesInitializer : IModuleInitializer
{
    /// <summary>
    /// Registers the business module's ViewModels and Views with the dependency injection service collection.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    /// <param name="configuration">The application configuration root providing configuration settings.</param>
    public void Initialize(IServiceCollection services, IConfiguration configuration)
    {
    }
}
