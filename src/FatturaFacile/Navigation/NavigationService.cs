// <copyright file="NavigationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.ViewModels;

namespace FatturaFacile.Wpf.Navigation;

/// <summary>
/// Implements <see cref="INavigationService"/> using Microsoft.Extensions.DependencyInjection to resolve view models dynamically.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NavigationService"/> class.
/// </remarks>
/// <param name="serviceProvider">The application service provider used to resolve view models.</param>
public class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private ViewModelBase? _currentViewModel;

    /// <inheritdoc />
    public ViewModelBase? CurrentViewModel => _currentViewModel;

    /// <inheritdoc />
    public event EventHandler<ViewModelBase>? Navigated;


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
