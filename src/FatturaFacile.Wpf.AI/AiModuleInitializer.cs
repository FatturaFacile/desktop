// <copyright file="AiModuleInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Services;
using FatturaFacile.ViewModels;
using FatturaFacile.Wpf.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.AI;

/// <summary>
/// Registers AI services, agents, tools, and viewmodels into dependency injection.
/// </summary>
public sealed class AiModuleInitializer : IModuleInitializer
{
    /// <inheritdoc />
    public void Initialize(
        IServiceCollection services,
        IConfiguration configuration)
    {
        // ViewModels
        services.AddTransient<AiToolingModuleViewModel>();
        services.AddTransient<PromptToExpressionModuleViewModel>();
        services.AddTransient<SmartPasteModuleViewModel>();
        services.AddTransient<BlazorHybridViewModel>();

        // Services
        services.AddSingleton<WeatherForecastService>();
    }
}
