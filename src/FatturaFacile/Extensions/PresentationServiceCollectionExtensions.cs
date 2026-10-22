// <copyright file="PresentationServiceCollectionExtensions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.ViewModels;
using FatturaFacile.Wpf.Common.Contracts;
using FatturaFacile.Wpf.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Extensions;

/// <summary>
/// Metodi di estensione per la registrazione dei servizi e dei ViewModel del layer di presentazione.
/// </summary>
public static class PresentationServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        // 1. Servizi di presentazione e gestione stato utente
        services.AddSingleton<IUserStateService, UserStateService>();
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();

        // 2. Registrazione dei sotto-ViewModel (risolti on-demand tramite IViewModelFactory)
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ClientsViewModel>();
        services.AddTransient<InvoicesViewModel>();
        services.AddTransient<TireHotelViewModel>();
        services.AddTransient<TimeTrackingViewModel>();
        services.AddTransient<LocalizationAdminViewModel>();
        services.AddTransient<LoginViewModel>();

        // 3. Shell e Finestra Principale
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}