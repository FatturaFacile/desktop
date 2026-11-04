// <copyright file="INavigationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Desktop.Wpf.ViewModels;

namespace FatturaFacile.Desktop.Wpf.Services;

/// <summary>
/// Navigation contract for switching views and workspaces within the desktop ERP client.
/// </summary>
internal interface INavigationService
{
    /// <summary>
    /// Occurs when the active view model changes.
    /// </summary>
    public event EventHandler? CurrentViewModelChanged;

    /// <summary>
    /// Gets the current active view model displayed in the workspace shell.
    /// </summary>
    public ViewModelBase? CurrentViewModel { get; }

    /// <summary>
    /// Navigates to a view model resolved from the dependency injection service container.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type to activate.</typeparam>
    public void NavigateTo<TViewModel>()
        where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigates to a specific view model instance.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    public void NavigateTo(ViewModelBase viewModel);
}
