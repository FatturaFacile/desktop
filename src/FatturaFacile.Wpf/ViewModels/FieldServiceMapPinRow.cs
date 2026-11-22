// <copyright file="FieldServiceMapPinRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A pin of the dispatcher map: the site of a work order.
/// </summary>
internal sealed partial class FieldServiceMapPinRow : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceMapPinRow"/> class.
    /// </summary>
    /// <param name="workOrderId">The work order identifier.</param>
    /// <param name="number">The work order number.</param>
    /// <param name="title">The work order title.</param>
    /// <param name="left">The offset of the pin from the left edge of the map, in device-independent pixels.</param>
    /// <param name="top">The offset of the pin from the top edge of the map, in device-independent pixels.</param>
    /// <param name="color">The color of the pin (the technician route, or the unassigned color).</param>
    /// <param name="tooltip">The tooltip.</param>
    /// <param name="isUnassigned">Whether the work order is waiting for a technician.</param>
    public FieldServiceMapPinRow(string workOrderId, string number, string title, double left, double top, string color, string tooltip, bool isUnassigned)
    {
        WorkOrderId = workOrderId;
        Number = number;
        Title = title;
        Left = left;
        Top = top;
        Color = color;
        Tooltip = tooltip;
        IsUnassigned = isUnassigned;
    }

    /// <summary>
    /// Gets the work order identifier.
    /// </summary>
    public string WorkOrderId { get; }

    /// <summary>
    /// Gets the work order number.
    /// </summary>
    public string Number { get; }

    /// <summary>
    /// Gets the work order title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the offset of the pin from the left edge of the map.
    /// </summary>
    public double Left { get; }

    /// <summary>
    /// Gets the offset of the pin from the top edge of the map.
    /// </summary>
    public double Top { get; }

    /// <summary>
    /// Gets the color of the pin.
    /// </summary>
    public string Color { get; }

    /// <summary>
    /// Gets the tooltip.
    /// </summary>
    public string Tooltip { get; }

    /// <summary>
    /// Gets a value indicating whether the work order is waiting for a technician.
    /// </summary>
    public bool IsUnassigned { get; }
}
