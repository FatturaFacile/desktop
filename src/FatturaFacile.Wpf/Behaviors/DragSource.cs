// <copyright file="DragSource.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Lets the items of a list or a data grid be dragged: pressing the left button on an item and moving the pointer starts a drag
/// that carries the item (the data context of the row) for a <see cref="DropTarget"/>.
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class DragSource
{
    /// <summary>
    /// The data format the dragged item travels in.
    /// </summary>
    public const string Format = "FatturaFacile.DragItem";

    /// <summary>
    /// Identifies the IsEnabled attached property.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(DragSource),
        new PropertyMetadata(defaultValue: false, OnIsEnabledChanged));

    private static readonly ConditionalWeakTable<ItemsControl, PendingDrag> Pending = new();

    /// <summary>
    /// Gets a value indicating whether the items of the control can be dragged.
    /// </summary>
    /// <param name="target">The control.</param>
    /// <returns>The flag.</returns>
    public static bool GetIsEnabled(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (bool)target.GetValue(IsEnabledProperty);
    }

    /// <summary>
    /// Sets a value indicating whether the items of the control can be dragged.
    /// </summary>
    /// <param name="target">The control.</param>
    /// <param name="value">The flag.</param>
    public static void SetIsEnabled(DependencyObject target, bool value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not ItemsControl control)
        {
            return;
        }

        control.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        control.PreviewMouseMove -= OnPreviewMouseMove;
        if (e.NewValue is true)
        {
            control.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            control.PreviewMouseMove += OnPreviewMouseMove;
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ItemsControl control)
        {
            return;
        }

        var container = e.OriginalSource is DependencyObject source ? ItemsControl.ContainerFromElement(control, source) : null;
        var item = container is null ? null : control.ItemContainerGenerator.ItemFromContainer(container);
        _ = Pending.Remove(control);
        if (item is not null && item != DependencyProperty.UnsetValue)
        {
            Pending.Add(control, new PendingDrag(item, e.GetPosition(control)));
        }
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not ItemsControl control || e.LeftButton != MouseButtonState.Pressed || !Pending.TryGetValue(control, out var drag))
        {
            return;
        }

        var position = e.GetPosition(control);
        if (Math.Abs(position.X - drag.Start.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(position.Y - drag.Start.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _ = Pending.Remove(control);
        _ = DragDrop.DoDragDrop(control, new DataObject(Format, drag.Item), DragDropEffects.Move);
    }

    private sealed record PendingDrag(object Item, Point Start);
}
