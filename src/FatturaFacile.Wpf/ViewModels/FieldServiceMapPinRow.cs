// <copyright file="FieldServiceMapPinRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A pin of the dispatcher map: the site of a work order.
/// </summary>
/// <param name="workOrderId">The work order identifier.</param>
/// <param name="number">The work order number.</param>
/// <param name="title">The work order title.</param>
/// <param name="position">The offset of the pin from the top-left corner of the map, in device-independent pixels.</param>
/// <param name="color">The color of the pin (the technician route, or the unassigned color).</param>
/// <param name="tooltip">The tooltip.</param>
/// <param name="isUnassigned">Whether the work order is waiting for a technician.</param>
internal sealed partial class FieldServiceMapPinRow(
    string workOrderId,
    string number,
    string title,
    FieldServiceMapPinPosition position,
    string color,
    string tooltip,
    bool isUnassigned) : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Gets the work order identifier.
    /// </summary>
    public string WorkOrderId { get; } = workOrderId;

    /// <summary>
    /// Gets the work order number.
    /// </summary>
    public string Number { get; } = number;

    /// <summary>
    /// Gets the work order title.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Gets the offset of the pin from the left edge of the map.
    /// </summary>
    public double Left { get; } = position.Left;

    /// <summary>
    /// Gets the offset of the pin from the top edge of the map.
    /// </summary>
    public double Top { get; } = position.Top;

    /// <summary>
    /// Gets the color of the pin.
    /// </summary>
    public string Color { get; } = color;

    /// <summary>
    /// Gets the tooltip.
    /// </summary>
    public string Tooltip { get; } = tooltip;

    /// <summary>
    /// Gets a value indicating whether the work order is waiting for a technician.
    /// </summary>
    public bool IsUnassigned { get; } = isUnassigned;
}
