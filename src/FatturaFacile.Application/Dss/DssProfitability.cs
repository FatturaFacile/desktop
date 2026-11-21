// <copyright file="DssProfitability.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// The profitability matrix and Whale Curve of one dimension.
/// </summary>
/// <param name="Dimension">The dimension (customer, product, ...).</param>
/// <param name="Rows">The ranked rows.</param>
/// <param name="WhaleCurve">The Whale Curve series.</param>
/// <param name="ParetoCutoffRank">The rank at which the 80/20 core ends, or zero.</param>
public sealed record DssProfitability(
    string Dimension,
    IReadOnlyList<DssMatrixRow> Rows,
    IReadOnlyList<DssWhalePoint> WhaleCurve,
    int ParetoCutoffRank);
