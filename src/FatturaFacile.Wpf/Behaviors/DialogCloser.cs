// <copyright file="DialogCloser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Closes a dialog window from its view model: binding DialogResult to a non-null value sets the window result.
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class DialogCloser
{
    /// <summary>
    /// Identifies the DialogResult attached property.
    /// </summary>
    public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
        "DialogResult",
        typeof(bool?),
        typeof(DialogCloser),
        new PropertyMetadata(defaultValue: null, OnDialogResultChanged));

    /// <summary>
    /// Gets the dialog result.
    /// </summary>
    /// <param name="target">The window.</param>
    /// <returns>The dialog result.</returns>
    public static bool? GetDialogResult(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (bool?)target.GetValue(DialogResultProperty);
    }

    /// <summary>
    /// Sets the dialog result.
    /// </summary>
    /// <param name="target">The window.</param>
    /// <param name="value">The dialog result.</param>
    public static void SetDialogResult(DependencyObject target, bool? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(DialogResultProperty, value);
    }

    private static void OnDialogResultChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window { IsLoaded: true } window && e.NewValue is bool result)
        {
            window.DialogResult = result;
        }
    }
}
