// <copyright file="RebateTierMode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// How the scale of a tiered volume rebate is applied to the qualifying spend.
/// </summary>
public enum RebateTierMode
{
    /// <summary>
    /// The rate of the highest tier reached applies to the whole volume.
    /// </summary>
    Retroactive = 0,

    /// <summary>
    /// Every band of volume between two thresholds earns the rate of its own tier.
    /// </summary>
    Incremental = 1,
}
