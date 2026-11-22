// <copyright file="CountSessionEventArgs.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Carries the count session an event is about.
/// </summary>
/// <param name="session">The count session.</param>
internal sealed class CountSessionEventArgs(CountSessionRow session) : EventArgs
{
    /// <summary>
    /// Gets the count session.
    /// </summary>
    public CountSessionRow Session { get; } = session;
}
