// <copyright file="ProjectType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The kind of a project (Mod 078).
/// </summary>
public enum ProjectType
{
    /// <summary>
    /// Work billed to a customer.
    /// </summary>
    ClientBillable = 0,

    /// <summary>
    /// Internal research and development.
    /// </summary>
    InternalRAndD = 1,

    /// <summary>
    /// An internal investment capitalized as an asset.
    /// </summary>
    CapexInvestment = 2,
}
