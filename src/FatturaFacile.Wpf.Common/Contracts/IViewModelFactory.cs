// <copyright file="IViewModelFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common.Contracts;

/// <summary>
/// Centralized factory for on-demand resolution and lifecycle creation of ViewModels.
/// </summary>
public interface IViewModelFactory
{

    /// <summary>
    /// Resolves and returns an instance of the requested ViewModel type from the dependency injection container.
    /// </summary>
    /// <typeparam name="TViewModel">The specific ViewModel class type to resolve.</typeparam>
    /// <returns>A resolved instance of <typeparamref name="TViewModel"/>.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no service for type <typeparamref name="TViewModel"/> is registered in the service provider.</exception>
    TViewModel Create<TViewModel>()
        where TViewModel : class;

}
