// <copyright file="DropTarget.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using FatturaFacile.Wpf.ViewModels;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Makes an element a place an item dragged from a <see cref="DragSource"/> is dropped on: the command receives a
/// <see cref="DropRequest"/> with the item and the data context of the element. While an item is over the element,
/// <see cref="IsDragOverProperty"/> is true so the style can highlight it.
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class DropTarget
{
    /// <summary>
    /// Identifies the Command attached property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(DropTarget),
        new PropertyMetadata(defaultValue: null, OnCommandChanged));

    /// <summary>
    /// Identifies the IsDragOver attached property.
    /// </summary>
    public static readonly DependencyProperty IsDragOverProperty = DependencyProperty.RegisterAttached(
        "IsDragOver",
        typeof(bool),
        typeof(DropTarget),
        new PropertyMetadata(defaultValue: false));

    /// <summary>
    /// Gets the command that receives the drop.
    /// </summary>
    /// <param name="target">The element.</param>
    /// <returns>The command.</returns>
    public static ICommand? GetCommand(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (ICommand?)target.GetValue(CommandProperty);
    }

    /// <summary>
    /// Sets the command that receives the drop.
    /// </summary>
    /// <param name="target">The element.</param>
    /// <param name="value">The command.</param>
    public static void SetCommand(DependencyObject target, ICommand? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether an item is being dragged over the element.
    /// </summary>
    /// <param name="target">The element.</param>
    /// <returns>The flag.</returns>
    public static bool GetIsDragOver(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (bool)target.GetValue(IsDragOverProperty);
    }

    /// <summary>
    /// Sets a value indicating whether an item is being dragged over the element.
    /// </summary>
    /// <param name="target">The element.</param>
    /// <param name="value">The flag.</param>
    public static void SetIsDragOver(DependencyObject target, bool value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(IsDragOverProperty, value);
    }

    private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not UIElement element)
        {
            return;
        }

        element.DragEnter -= OnDragOver;
        element.DragOver -= OnDragOver;
        element.DragLeave -= OnDragLeave;
        element.Drop -= OnDrop;
        element.AllowDrop = e.NewValue is not null;
        if (e.NewValue is not null)
        {
            element.DragEnter += OnDragOver;
            element.DragOver += OnDragOver;
            element.DragLeave += OnDragLeave;
            element.Drop += OnDrop;
        }
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        var accepted = e.Data.GetDataPresent(DragSource.Format);
        e.Effects = accepted ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
        if (sender is DependencyObject element)
        {
            SetIsDragOver(element, accepted);
        }
    }

    private static void OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is DependencyObject element)
        {
            SetIsDragOver(element, value: false);
        }

        e.Handled = true;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        SetIsDragOver(element, value: false);
        e.Handled = true;
        if (e.Data.GetData(DragSource.Format) is not { } payload || GetCommand(element) is not { } command)
        {
            return;
        }

        var request = new DropRequest(payload, element.DataContext);
        if (command.CanExecute(request))
        {
            command.Execute(request);
        }
    }
}
