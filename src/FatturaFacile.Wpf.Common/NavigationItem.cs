// <copyright file="NavigationItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common;

/// <summary>
/// Default implementation of <see cref="INavigationItem"/> representing a single navigation entry.
/// </summary>
/// <param name="caption">The user-facing title or caption for the navigation item.</param>
public class NavigationItem(string caption) : INavigationItem
{

    /// <inheritdoc />
    public string Caption { get; } = caption;

}
