// <copyright file="DirectVsApportionedRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// One center of the direct-versus-apportioned cost comparison.
/// </summary>
/// <param name="CenterId">The center identifier.</param>
/// <param name="Code">The center code.</param>
/// <param name="Name">The center name.</param>
/// <param name="Depth">The depth in the tree.</param>
/// <param name="DirectCost">The direct actual cost.</param>
/// <param name="ApportionedIn">The step-down inflows.</param>
/// <param name="ApportionedOut">The step-down outflows.</param>
/// <param name="NetCost">The net analytical cost.</param>
/// <param name="Variance">The net cost minus the direct cost.</param>
/// <param name="SharePercent">The share of the total net cost.</param>
public sealed record DirectVsApportionedRow(
    string CenterId,
    string Code,
    string Name,
    int Depth,
    decimal DirectCost,
    decimal ApportionedIn,
    decimal ApportionedOut,
    decimal NetCost,
    decimal Variance,
    decimal SharePercent);
