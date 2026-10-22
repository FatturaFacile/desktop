// <copyright file="IViewModelFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm;

namespace FatturaFacile.Wpf.Common.Contracts;

/// <summary>
/// Factory centralizzata per la creazione e risoluzione on-demand dei ViewModel.
/// </summary>
public interface IViewModelFactory
{
    TViewModel Create<TViewModel>() where TViewModel : ViewModelBase;
}