// <copyright file="CalculateSwissVatReturnOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Combined calculation and quadratura output returned by <see cref="CalculateSwissVatReturnCommandHandler"/>.
/// </summary>
/// <param name="Calculation">Computed AFC boxes and summary figures.</param>
/// <param name="Quadratura">Quadratura Q1..Q9 report.</param>
public sealed record CalculateSwissVatReturnOutcome(
    SwissVatCalculationResult Calculation,
    QuadraturaReportDto Quadratura);
