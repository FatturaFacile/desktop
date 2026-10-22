// <copyright file="ViewModelFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm;
using FatturaFacile.Wpf.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Factory centralizzata per la creazione e risoluzione on-demand dei ViewModel.
/// </summary>
public sealed class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelFactory(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TViewModel Create<TViewModel>() where TViewModel : class =>
        _serviceProvider.GetRequiredService<TViewModel>();
}
