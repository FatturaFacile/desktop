// <copyright file="NavigationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Desktop.Wpf.ViewModels;

namespace FatturaFacile.Desktop.Wpf.Services;

/// <summary>
/// Default in-memory desktop navigation service coordinating active views and tabs.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NavigationService"/> class.
/// </remarks>
/// <param name="serviceProvider">The application service provider for resolving view models.</param>
internal sealed class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public event EventHandler? CurrentViewModelChanged;

    /// <inheritdoc />
    public ViewModelBase? CurrentViewModel { get; private set; }

    /// <inheritdoc />
    public void NavigateTo<TViewModel>()
        where TViewModel : ViewModelBase
    {
        var vm = (ViewModelBase)_serviceProvider.GetService(typeof(TViewModel))!;
        NavigateTo(vm);
    }

    /// <inheritdoc />
    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
        CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
    }
}
