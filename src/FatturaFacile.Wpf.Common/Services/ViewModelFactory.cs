// <copyright file="ViewModelFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Centralized factory for on-demand creation and dependency resolution of ViewModels.
/// </summary>
public sealed class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve ViewModel dependencies.</param>
    public ViewModelFactory(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TViewModel Create<TViewModel>() where TViewModel : class =>
        _serviceProvider.GetRequiredService<TViewModel>();
}
