// <copyright file="ViewModelBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Base class for all desktop view models providing observable properties and busy states.
/// </summary>
internal abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;
}
