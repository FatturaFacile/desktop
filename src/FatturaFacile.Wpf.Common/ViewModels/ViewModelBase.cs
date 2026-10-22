// <copyright file="ViewModelBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Abstract base class for ViewModels providing common observability, state flags, and initialization hooks.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{

    /// <summary>
    /// Gets or sets a value indicating whether an asynchronous or background operation is actively in progress.
    /// </summary>
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    /// <summary>
    /// Gets or sets the current user-facing error message, or <see langword="null"/> when no error is present.
    /// </summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>
    /// Asynchronously initializes view model data, commands, or underlying business states after instantiation.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization lifecycle.</returns>
    public virtual Task InitializeAsync() => Task.CompletedTask;

}
