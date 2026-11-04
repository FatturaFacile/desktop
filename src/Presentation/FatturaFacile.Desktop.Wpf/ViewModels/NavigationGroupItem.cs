// <copyright file="NavigationGroupItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Navigation group representing an accordion or sidebar category.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NavigationGroupItem"/> class.
/// </remarks>
/// <param name="header">The localized group header title.</param>
/// <param name="icon">The icon identifier.</param>
internal sealed class NavigationGroupItem(string header, string icon)
{
    /// <summary>
    /// Gets the group category header label.
    /// </summary>
    public string Header { get; } = header;

    /// <summary>
    /// Gets the vector icon name for the category.
    /// </summary>
    public string Icon { get; } = icon;

    /// <summary>
    /// Gets the child navigation items belonging to this category.
    /// </summary>
    public ObservableCollection<NavigationLeafItem> Items { get; } = [];
}
