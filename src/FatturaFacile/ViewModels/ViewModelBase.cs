// <copyright file="ViewModelBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Abstract base class for presentation view models providing notification support, busy indication, and async lifecycle initialization.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string? _busyMessage;

    /// <summary>
    /// Gets or sets a value indicating whether an asynchronous background operation is actively executing.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Gets or sets the descriptive busy or progress message displayed in the UI overlay.
    /// </summary>
    public string? BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }

    /// <summary>
    /// Performs asynchronous data loading, command query dispatch, or state initialization for the view model.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}
