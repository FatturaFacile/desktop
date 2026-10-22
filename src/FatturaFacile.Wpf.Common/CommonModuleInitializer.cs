// <copyright file="CommonModuleInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.Common.Contracts;
using FatturaFacile.Wpf.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Common;

/// <summary>
/// Initializes common WPF presentation infrastructure services.
/// </summary>
public sealed class CommonModuleInitializer : IModuleInitializer
{
    /// <inheritdoc />
    public void Initialize(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IUserStateService, UserStateService>();
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
    }
}
