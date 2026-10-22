// <copyright file="INavigationItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common;

/// <summary>
/// Contract representing an item displayed in the navigation pane or menu.
/// </summary>
public interface INavigationItem
{

    /// <summary>
    /// Gets the user-facing caption of the navigation item.
    /// </summary>
    string Caption { get; }

}
