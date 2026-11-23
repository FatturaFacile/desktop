// <copyright file="SwissVatQuadraturaTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests for <see cref="SwissVatQuadraturaValidator"/>.
/// </summary>
public sealed class SwissVatQuadraturaTests
{
    /// <summary>
    /// Verifies that a well-formed return passes all Q1..Q9 checks with zero blockers.
    /// </summary>
    [Fact]
    public void Validate_ConsistentReturnWithValidUid_PassesAllChecks()
    {
        SwissVatCalculationInput input = new(
            PeriodFrom: new DateOnly(2026, 1, 1),
            PeriodTo: new DateOnly(2026, 3, 31),
            Method: SwissVatMethod.Effective,
            ConsiderationBasis: SwissVatConsiderationBasis.Agreed,
            SalesLines:
            [
                new SwissVatSaleLineInput(5000m, 8.10m, AfcBox: "303"),
                new SwissVatSaleLineInput(1000m, 0m, ExemptionType: SwissVatExemptionType.ExemptWithDeduction, AfcBox: "220"),
            ],
            PurchaseLines: [new SwissVatPurchaseLineInput(1000m, 81m, "400", true)],
            BezugsteuerEntries: []);

        var calc = SwissVatCalculator.Calculate(input);
        var report = SwissVatQuadraturaValidator.Validate(calc, "CHE-116.281.710 MWST", pendingDetectedBezugsteuerCount: 0);

        Assert.True(report.Passed);
        Assert.Equal(0, report.BlockerCount);
        Assert.Equal(0, report.WarningCount);
        Assert.Equal(9, report.Checks.Count);
    }

    /// <summary>
    /// Verifies that an inconsistent Box 299 balance triggers a Q1 blocker and missing UID triggers Q8 warning.
    /// </summary>
    [Fact]
    public void Validate_TamperedTurnoverAndInvalidUid_FlagsBlockerAndWarning()
    {
        SwissVatCalculationInput input = new(
            PeriodFrom: new DateOnly(2026, 1, 1),
            PeriodTo: new DateOnly(2026, 3, 31),
            Method: SwissVatMethod.Effective,
            ConsiderationBasis: SwissVatConsiderationBasis.Agreed,
            SalesLines: [new SwissVatSaleLineInput(5000m, 8.10m, AfcBox: "303")],
            PurchaseLines: [],
            BezugsteuerEntries: []);

        var calc = SwissVatCalculator.Calculate(input) with { Box299TaxableTurnover = 1234m };
        var report = SwissVatQuadraturaValidator.Validate(calc, "invalid-uid", pendingDetectedBezugsteuerCount: 1);

        Assert.False(report.Passed);
        Assert.True(report.BlockerCount >= 1);
        Assert.Equal(2, report.WarningCount);
    }
}
