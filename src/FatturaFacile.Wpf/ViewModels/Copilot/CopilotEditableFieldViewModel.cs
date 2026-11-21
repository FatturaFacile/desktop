// <copyright file="CopilotEditableFieldViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// One argument of a proposed action the user may change before confirming (listed in "editable_fields").
/// </summary>
/// <param name="path">The dotted argument path.</param>
/// <param name="value">The current value.</param>
internal sealed partial class CopilotEditableFieldViewModel(string path, string value) : ObservableObject
{
    [ObservableProperty]
    private string _value = value;

    /// <summary>
    /// Gets the dotted argument path, e.g. "items.0.quantity".
    /// </summary>
    public string Path { get; } = path;
}
