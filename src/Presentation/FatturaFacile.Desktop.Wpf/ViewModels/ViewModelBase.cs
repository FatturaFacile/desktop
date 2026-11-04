// <copyright file="ViewModelBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Base class for all desktop view models providing observable properties and busy states.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    private string _title = string.Empty;

    private bool _isBusy;

    private string? _statusMessage;

    /// <summary>
    /// Gets or sets the display title of the view or workspace tab.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether an asynchronous background task is in progress.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Gets or sets an optional contextual status message displayed to the user.
    /// </summary>
    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
}
