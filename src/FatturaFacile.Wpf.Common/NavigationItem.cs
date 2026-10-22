// <copyright file="NavigationItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace Common;

/// <summary>
/// Default implementation of <see cref="INavigationItem"/>.
/// </summary>
/// <param name="caption">The display caption for the navigation item.</param>
public class NavigationItem(string caption) : INavigationItem
{
    public string Caption { get; } = caption;
}
