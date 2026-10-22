// <copyright file="ViewModelFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Common.Services;

public sealed class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TViewModel Create<TViewModel>() where TViewModel : ViewModelBase
    {
        return _serviceProvider.GetRequiredService<TViewModel>();
    }
}