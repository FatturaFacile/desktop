// <copyright file="PresentationServiceCollectionExtensions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Reflection;

using FatturaFacile.ViewModels;
using FatturaFacile.Wpf.AI;
using FatturaFacile.Wpf.Common;
using FatturaFacile.Wpf.Modules;
using FatturaFacile.Wpf.Reporting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Extensions;

/// <summary>
/// Provides extension methods for registering presentation layer services, module initializers, and main window shell components.
/// </summary>
public static class PresentationServiceCollectionExtensions
{

    /// <summary>
    /// Discovers and executes all <see cref="IModuleInitializer"/> instances across presentation and UI feature assemblies,
    /// and registers shell view models and windows into the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The target service collection to receive dependency registrations.</param>
    /// <param name="configuration">The root configuration provider supplying settings to initialized modules.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent method chaining.</returns>
    public static IServiceCollection AddPresentationServices(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        // 1. Automatic module registration via IModuleInitializer
        var assemblies = new Assembly[]
                         {
                             typeof(CommonModuleInitializer).Assembly, typeof(BusinessModulesInitializer).Assembly,
                             typeof(ReportingModuleInitializer).Assembly, typeof(AiModuleInitializer).Assembly
                         };

        var initializerType = typeof(IModuleInitializer);

        foreach (var assembly in assemblies)
        {
            var initializers = assembly.GetTypes()
                                       .Where(predicate: t => initializerType.IsAssignableFrom(c: t) && !t.IsInterface &&
                                                              !t.IsAbstract);

            foreach (var type in initializers)
            {
                var initializer = (IModuleInitializer)Activator.CreateInstance(type: type)!;
                initializer.Initialize(services: services, configuration: configuration);
            }
        }

        // 2. Shell and Main Window (Composition Root)
        _ = services.AddSingleton<MainViewModel>();
        _ = services.AddSingleton<MainWindow>();

        return services;
    }

}
