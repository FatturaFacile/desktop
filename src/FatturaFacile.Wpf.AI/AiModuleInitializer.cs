// <copyright file="AiModuleInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.AI.Services;
using FatturaFacile.Wpf.AI.ViewModels;
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
        IConfiguration     configuration)
    {
        // ViewModels
        _ = services.AddTransient<AiToolingModuleViewModel>();
        _ = services.AddTransient<PromptToExpressionModuleViewModel>();
        _ = services.AddTransient<SmartPasteModuleViewModel>();
        _ = services.AddTransient<BlazorHybridViewModel>();

        // Services
        _ = services.AddSingleton<WeatherForecastService>();
    }

}
