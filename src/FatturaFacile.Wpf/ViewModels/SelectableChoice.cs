// <copyright file="SelectableChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An entry of a multiple choice list of the resource planning views: a stable key with its label and a check box.
/// </summary>
internal sealed partial class SelectableChoice : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectableChoice"/> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="label">The label.</param>
    public SelectableChoice(string key, string label)
    {
        Key = key;
        Label = label;
    }

    /// <summary>
    /// Gets the key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the label.
    /// </summary>
    public string Label { get; }
}
