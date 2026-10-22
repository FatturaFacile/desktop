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
/// Metodi di estensione per la registrazione modulare dei servizi e dei componenti di presentazione.
/// </summary>
public static class PresentationServiceCollectionExtensions
{
    /// <summary>
    /// Registra i moduli di presentazione tramite scansione e inizializzazione di <see cref="IModuleInitializer"/>.
    /// </summary>
    public static IServiceCollection AddPresentationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Registrazione automatica dei moduli tramite IModuleInitializer
        var assemblies = new Assembly[]
        {
            typeof(CommonModuleInitializer).Assembly,
            typeof(BusinessModulesInitializer).Assembly,
            typeof(ReportingModuleInitializer).Assembly,
            typeof(AiModuleInitializer).Assembly
        };

        var initializerType = typeof(IModuleInitializer);
        foreach (var assembly in assemblies)
        {
            var initializers = assembly.GetTypes()
                .Where(t => initializerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in initializers)
            {
                var initializer = (IModuleInitializer)Activator.CreateInstance(type)!;
                initializer.Initialize(services, configuration);
            }
        }

        // 2. Shell e Finestra Principale (Composition Root)
        _ = services.AddSingleton<MainViewModel>();
        _ = services.AddSingleton<MainWindow>();

        return services;
    }
}
