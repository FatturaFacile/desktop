// <copyright file="FieldServiceChecklistEditRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An editable checklist line of the service report being signed.
/// </summary>
internal sealed partial class FieldServiceChecklistEditRow : ObservableObject
{
    [ObservableProperty]
    private string _result = "pass";

    [ObservableProperty]
    private string _notes = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceChecklistEditRow"/> class.
    /// </summary>
    /// <param name="item">The checklist item sent to the server.</param>
    /// <param name="label">The localized label shown to the technician.</param>
    public FieldServiceChecklistEditRow(string item, string label)
    {
        Item = item;
        Label = label;
    }

    /// <summary>
    /// Gets the checklist item sent to the server.
    /// </summary>
    public string Item { get; }

    /// <summary>
    /// Gets the localized label shown to the technician.
    /// </summary>
    public string Label { get; }
}
