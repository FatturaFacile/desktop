// <copyright file="IApplicationRestarter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// Restarts the application (needed after a restore, which replaces the database and the key).
/// </summary>
internal interface IApplicationRestarter
{
    /// <summary>
    /// Starts a new instance and shuts the current one down.
    /// </summary>
    public void Restart();
}
