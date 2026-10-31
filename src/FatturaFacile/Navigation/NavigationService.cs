// <copyright file="NavigationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Navigation;

/// <summary>
/// Implements <see cref="INavigationService"/> using Microsoft.Extensions.DependencyInjection to resolve view models dynamically.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ViewModelBase? _currentViewModel;

    /// <inheritdoc />
    public ViewModelBase? CurrentViewModel => _currentViewModel;

    /// <inheritdoc />
    public event EventHandler<ViewModelBase>? Navigated;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The application service provider used to resolve view models.</param>
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        NavigateTo(typeof(TViewModel));
    }

    /// <inheritdoc />
    public void NavigateTo(Type viewModelType)
    {
        var vm = (ViewModelBase)_serviceProvider.GetRequiredService(viewModelType);
        _currentViewModel = vm;
        _ = vm.InitializeAsync();
        Navigated?.Invoke(this, vm);
    }
}
