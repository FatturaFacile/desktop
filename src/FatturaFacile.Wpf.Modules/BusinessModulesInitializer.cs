// <copyright file="BusinessModulesInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.ViewModels;
using FatturaFacile.Views;
using FatturaFacile.Wpf.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Modules;

/// <summary>
/// Registers core business modules, views, and viewmodels in dependency injection.
/// </summary>
public sealed class BusinessModulesInitializer : IModuleInitializer
{
    /// <inheritdoc />
    public void Initialize(IServiceCollection services, IConfiguration configuration)
    {
        // Business ViewModels (transient for on-demand resolution via IViewModelFactory)
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ClientsViewModel>();
        services.AddTransient<InvoicesViewModel>();
        services.AddTransient<TireHotelViewModel>();
        services.AddTransient<TimeTrackingViewModel>();
        services.AddTransient<LocalizationAdminViewModel>();
        services.AddTransient<LoginViewModel>();

        // Views
        services.AddTransient<DashboardView>();
        services.AddTransient<ClientsView>();
        services.AddTransient<InvoicesView>();
        services.AddTransient<TireHotelView>();
        services.AddTransient<TimeTrackingView>();
        services.AddTransient<LocalizationAdminView>();
        services.AddTransient<LoginDialog>();
    }
}
