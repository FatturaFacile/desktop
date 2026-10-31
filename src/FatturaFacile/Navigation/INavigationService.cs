// <copyright file="INavigationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Navigation;

/// <summary>
/// Provides client-side navigation abstraction for resolving and activating view models within the shell view host.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the currently active view model instance hosted in the presentation shell.
    /// </summary>
    ViewModelBase? CurrentViewModel { get; }

    /// <summary>
    /// Navigates to a view model of type <typeparamref name="TViewModel"/> resolved via dependency injection.
    /// </summary>
    /// <typeparam name="TViewModel">The type of <see cref="ViewModelBase"/> to navigate to.</typeparam>
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigates to a specified view model type resolved via dependency injection.
    /// </summary>
    /// <param name="viewModelType">The <see cref="Type"/> of view model to activate.</param>
    void NavigateTo(Type viewModelType);

    /// <summary>
    /// Event raised after navigation completes and the new view model has been initialized.
    /// </summary>
    event EventHandler<ViewModelBase>? Navigated;
}
