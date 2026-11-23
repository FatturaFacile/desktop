// <copyright file="QuadraturaReportDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Aggregated quadratura report across all Q1..Q9 checks.
/// </summary>
/// <param name="Passed">True when no blocker checks failed.</param>
/// <param name="Checks">All evaluated checks.</param>
/// <param name="BlockersCount">Number of failed blocker checks.</param>
/// <param name="WarningsCount">Number of failed warning checks.</param>
public sealed record QuadraturaReportDto(
    bool Passed,
    IReadOnlyList<QuadraturaCheckDto> Checks,
    int BlockersCount,
    int WarningsCount);
