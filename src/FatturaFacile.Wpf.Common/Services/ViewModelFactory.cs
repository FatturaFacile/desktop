// <copyright file="ViewModelFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.Common.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Centralized factory for on-demand creation and dependency resolution of ViewModels.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ViewModelFactory"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider used to resolve ViewModel dependencies.</param>
public sealed class ViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{

    /// <summary>
    /// The service provider used to resolve ViewModel dependencies.
    /// </summary>
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public TViewModel Create<TViewModel>()
        where TViewModel : class =>
        _serviceProvider.GetRequiredService<TViewModel>();

}
