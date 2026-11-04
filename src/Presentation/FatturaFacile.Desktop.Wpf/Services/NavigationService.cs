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
public sealed class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private ViewModelBase? _currentViewModel;

    /// <inheritdoc />
    public event Action<ViewModelBase>? CurrentViewModelChanged;

    /// <inheritdoc />
    public ViewModelBase? CurrentViewModel => _currentViewModel;

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
        _currentViewModel = viewModel;
        CurrentViewModelChanged?.Invoke(viewModel);
    }
}
