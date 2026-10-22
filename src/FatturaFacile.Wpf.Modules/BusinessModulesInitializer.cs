// <copyright file="BusinessModulesInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.ViewModels;
using FatturaFacile.Wpf.Common;
using FatturaFacile.Wpf.Modules.Views;
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
        // Business ViewModels (transient for on-demand resolution via IViewModelFactory)
        _ = services.AddTransient<DashboardViewModel>();
        _ = services.AddTransient<ClientsViewModel>();
        _ = services.AddTransient<InvoicesViewModel>();
        _ = services.AddTransient<TireHotelViewModel>();
        _ = services.AddTransient<TimeTrackingViewModel>();
        _ = services.AddTransient<LocalizationAdminViewModel>();
        _ = services.AddTransient<LoginViewModel>();

        // Views
        _ = services.AddTransient<DashboardView>();
        _ = services.AddTransient<ClientsView>();
        _ = services.AddTransient<InvoicesView>();
        _ = services.AddTransient<TireHotelView>();
        _ = services.AddTransient<TimeTrackingView>();
        _ = services.AddTransient<LocalizationAdminView>();
        _ = services.AddTransient<LoginDialog>();
    }

}
