// <copyright file="BubbleMouseWheel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Lets the mouse wheel scroll the surrounding scroll viewer when the pointer is over a control that would swallow
/// it (a read-only flow document or a data grid embedded in a message).
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class BubbleMouseWheel
{
    /// <summary>
    /// Identifies the IsEnabled attached property.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(BubbleMouseWheel),
        new PropertyMetadata(defaultValue: false, OnIsEnabledChanged));

    /// <summary>
    /// Gets a value indicating whether wheel events bubble to the parent.
    /// </summary>
    /// <param name="target">The element.</param>
    /// <returns>The flag.</returns>
    public static bool GetIsEnabled(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (bool)target.GetValue(IsEnabledProperty);
    }

    /// <summary>
    /// Sets a value indicating whether wheel events bubble to the parent.
    /// </summary>
    /// <param name="target">The element.</param>
    /// <param name="value">The flag.</param>
    public static void SetIsEnabled(DependencyObject target, bool value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not UIElement element)
        {
            return;
        }

        element.PreviewMouseWheel -= OnPreviewMouseWheel;

        if (e.NewValue is true)
        {
            element.PreviewMouseWheel += OnPreviewMouseWheel;
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled || sender is not DependencyObject element || VisualTreeHelper.GetParent(element) is not UIElement parent)
        {
            return;
        }

        e.Handled = true;
        MouseWheelEventArgs forwarded = new(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent,
            Source = sender,
        };
        parent.RaiseEvent(forwarded);
    }
}
