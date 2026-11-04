// <copyright file="NavigationLeafItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Navigation leaf item representing a clickable destination view.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NavigationLeafItem"/> class.
/// </remarks>
/// <param name="title">The localized destination title.</param>
/// <param name="target">The target route identifier.</param>
/// <param name="icon">The icon identifier.</param>
internal sealed class NavigationLeafItem(string title, string target, string icon)
{
    /// <summary>
    /// Gets the localized title of the navigation destination.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Gets the target navigation key.
    /// </summary>
    public string Target { get; } = target;

    /// <summary>
    /// Gets the vector icon name.
    /// </summary>
    public string Icon { get; } = icon;
}
