// <copyright file="FieldServiceEquipmentPick.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A customer asset the user can tick as covered by the maintenance contract being edited.
/// </summary>
internal sealed partial class FieldServiceEquipmentPick : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceEquipmentPick"/> class.
    /// </summary>
    /// <param name="id">The equipment identifier.</param>
    /// <param name="label">The label shown (serial number and site).</param>
    public FieldServiceEquipmentPick(string id, string label)
    {
        Id = id;
        Label = label;
    }

    /// <summary>
    /// Gets the equipment identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the label shown.
    /// </summary>
    public string Label { get; }
}
