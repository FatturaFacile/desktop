// <copyright file="PasswordBoxBinding.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Attached property that makes <see cref="PasswordBox.Password"/> bindable, keeping views free of code-behind.
/// </summary>
[SuppressMessage("Design", "MA0182:Unused internal type", Justification = "Used from XAML.")]
internal static class PasswordBoxBinding
{
    /// <summary>
    /// Identifies the bindable Password attached property.
    /// </summary>
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached(
        "Password",
        typeof(string),
        typeof(PasswordBoxBinding),
        new FrameworkPropertyMetadata(defaultValue: null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

    private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached(
        "IsUpdating",
        typeof(bool),
        typeof(PasswordBoxBinding),
        new PropertyMetadata(defaultValue: false));

    /// <summary>
    /// Gets the bound password.
    /// </summary>
    /// <param name="element">The password box.</param>
    /// <returns>The password.</returns>
    public static string? GetPassword(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (string?)element.GetValue(PasswordProperty);
    }

    /// <summary>
    /// Sets the bound password.
    /// </summary>
    /// <param name="element">The password box.</param>
    /// <param name="value">The password.</param>
    public static void SetPassword(DependencyObject element, string? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(PasswordProperty, value);
    }

    private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox box)
        {
            return;
        }

        box.PasswordChanged -= OnPasswordBoxPasswordChanged;

        if (!(bool)box.GetValue(IsUpdatingProperty))
        {
            box.Password = e.NewValue as string ?? string.Empty;
        }

        box.PasswordChanged += OnPasswordBoxPasswordChanged;
    }

    private static void OnPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
    {
        var box = (PasswordBox)sender;
        box.SetValue(IsUpdatingProperty, value: true);
        SetPassword(box, box.Password);
        box.SetValue(IsUpdatingProperty, value: false);
    }
}
