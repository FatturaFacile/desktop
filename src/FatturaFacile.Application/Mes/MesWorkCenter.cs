// <copyright file="MesWorkCenter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// A work centre the operator can pick.
/// </summary>
/// <param name="Id">The work centre identifier.</param>
/// <param name="Code">The work centre code.</param>
/// <param name="Name">The work centre name.</param>
public sealed record MesWorkCenter(Guid Id, string Code, string Name)
{
    /// <summary>
    /// Gets the text of a picker.
    /// </summary>
    public string Display => Code + " · " + Name;
}
