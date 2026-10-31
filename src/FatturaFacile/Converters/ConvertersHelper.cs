// <copyright file="ConvertersHelper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Data;

namespace FatturaFacile.Wpf.Converters;

/// <summary>
/// Provides static access to shared WPF value converter instances for XAML bindings.
/// </summary>
public static class ConvertersHelper
{
    /// <summary>
    /// Gets the shared singleton instance of <see cref="NotNullToVisibilityConverter"/>.
    /// </summary>
    public static IValueConverter NotNullToVisibility { get; } = new NotNullToVisibilityConverter();
}
