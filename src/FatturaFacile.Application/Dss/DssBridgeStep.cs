// <copyright file="DssBridgeStep.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// A step of the variance waterfall.
/// </summary>
/// <param name="Key">The step key: baseline, volume, price, cogs, opex, fx or actual.</param>
/// <param name="IsTotal">Whether the step is a total (baseline or actual) rather than a change.</param>
/// <param name="Value">The value of the step.</param>
/// <param name="Cumulative">The running total after the step.</param>
public sealed record DssBridgeStep(string Key, bool IsTotal, double Value, double Cumulative);
