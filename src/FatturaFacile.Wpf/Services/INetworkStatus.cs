// <copyright file="INetworkStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// Reports whether the machine has a network connection, so features that need a live server can show an offline badge.
/// </summary>
internal interface INetworkStatus
{
    /// <summary>
    /// Occurs when the availability of the network changes.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Gets a value indicating whether a network connection is available.
    /// </summary>
    public bool IsOnline { get; }
}
