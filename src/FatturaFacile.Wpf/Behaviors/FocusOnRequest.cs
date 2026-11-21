// <copyright file="FocusOnRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Moves the keyboard focus to an element whenever the bound counter changes (e.g. the Copilot prompt on Ctrl+K).
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class FocusOnRequest
{
    /// <summary>
    /// Identifies the Request attached property.
    /// </summary>
    public static readonly DependencyProperty RequestProperty = DependencyProperty.RegisterAttached(
        "Request",
        typeof(int),
        typeof(FocusOnRequest),
        new PropertyMetadata(defaultValue: 0, OnRequestChanged));

    /// <summary>
    /// Gets the request counter.
    /// </summary>
    /// <param name="target">The element.</param>
    /// <returns>The counter.</returns>
    public static int GetRequest(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (int)target.GetValue(RequestProperty);
    }

    /// <summary>
    /// Sets the request counter.
    /// </summary>
    /// <param name="target">The element.</param>
    /// <param name="value">The counter.</param>
    public static void SetRequest(DependencyObject target, int value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(RequestProperty, value);
    }

    [SuppressMessage("Usage", "VSTHRD001:Avoid legacy thread switching APIs", Justification = "Plain WPF attached behavior without a JoinableTaskFactory; focus must run after layout at input priority.")]
    private static void OnRequestChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is UIElement element && (int)e.NewValue > 0)
        {
            _ = element.Dispatcher.BeginInvoke(DispatcherPriority.Input, () => _ = Keyboard.Focus(element));
        }
    }
}
