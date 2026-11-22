// <copyright file="ProjectEvmScurvePoint.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// The cumulative planned value, earned value and actual cost at the end of a period (S-curve).
/// </summary>
/// <param name="Date">The end of the period.</param>
/// <param name="Pv">The cumulative planned value.</param>
/// <param name="Ev">The cumulative earned value.</param>
/// <param name="Ac">The cumulative actual cost.</param>
public sealed record ProjectEvmScurvePoint(DateTime Date, double Pv, double Ev, double Ac);
