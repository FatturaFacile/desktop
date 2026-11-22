// <copyright file="ProjectEvmPayload.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// The three `data` objects of one profitability pull, stored as received.
/// </summary>
/// <param name="BreakdownJson">The WBS breakdown payload.</param>
/// <param name="WaterfallJson">The waterfall payload.</param>
/// <param name="ScurveJson">The S-curve payload.</param>
public sealed record ProjectEvmPayload(string BreakdownJson, string WaterfallJson, string ScurveJson);
