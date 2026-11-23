// <copyright file="QuadraturaCheckDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Single accounting quadratura invariant check result.
/// </summary>
/// <param name="Code">Check code (Q1..Q9).</param>
/// <param name="Severity">Severity ("blocker" or "warning").</param>
/// <param name="Passed">True if check passed.</param>
/// <param name="Expected">Expected value.</param>
/// <param name="Actual">Actual value.</param>
/// <param name="Difference">Absolute difference.</param>
/// <param name="MessageKey">Localization resource key.</param>
public sealed record QuadraturaCheckDto(
    string Code,
    string Severity,
    bool Passed,
    string Expected,
    string Actual,
    string Difference,
    string MessageKey);
