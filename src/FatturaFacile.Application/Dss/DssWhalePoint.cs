// <copyright file="DssWhalePoint.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// A point of the Whale Curve.
/// </summary>
/// <param name="Rank">The rank.</param>
/// <param name="Label">The entity name.</param>
/// <param name="NetOperatingProfit">The entity profit.</param>
/// <param name="CumulativeProfitPct">The cumulative profit as a percentage of the total.</param>
public sealed record DssWhalePoint(int Rank, string Label, double NetOperatingProfit, double CumulativeProfitPct);
