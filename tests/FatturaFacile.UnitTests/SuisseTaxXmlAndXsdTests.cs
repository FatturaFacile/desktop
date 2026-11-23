// <copyright file="SuisseTaxXmlAndXsdTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests for <see cref="SuisseTaxXmlBuilder"/> and <see cref="SuisseTaxXsdValidator"/>.
/// </summary>
public sealed class SuisseTaxXmlAndXsdTests
{
    /// <summary>
    /// Verifies generated eCH-0217 XML validates cleanly against the embedded XSD and business rules.
    /// </summary>
    [Fact]
    public void BuildAndValidateAll_ValidReturn_PassesXsdAndQuadratura()
    {
        SwissVatCalculationInput input = new(
            PeriodFrom: new DateOnly(2026, 1, 1),
            PeriodTo: new DateOnly(2026, 3, 31),
            Method: SwissVatMethod.Effective,
            ConsiderationBasis: SwissVatConsiderationBasis.Agreed,
            SalesLines: [new SwissVatSaleLineInput(10000m, 8.10m, AfcBox: "303")],
            PurchaseLines: [new SwissVatPurchaseLineInput(1000m, 81m, "400", true)],
            BezugsteuerEntries: []);

        var calc = SwissVatCalculator.Calculate(input);
        var quadratura = SwissVatQuadraturaValidator.Validate(calc, "CHE-116.281.710 MWST");
        var xml = SuisseTaxXmlBuilder.Build(calc, "CHE-116.281.710 MWST", "Helvetia AG", quadratura.Passed);
        var validation = SuisseTaxXsdValidator.ValidateAll(xml, "CHE-116.281.710 MWST", quadratura);

        Assert.True(validation.Valid);
        Assert.True(validation.XsdValid);
        Assert.True(validation.QuadraturaPassed);
        Assert.Empty(validation.Errors);
        Assert.Contains("http://www.ech.ch/xmlns/eCH-0217/2", xml, StringComparison.Ordinal);
        Assert.Contains("CHE-116.281.710 MWST", xml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies malformed XML fails XSD schema validation with non-empty errors.
    /// </summary>
    [Fact]
    public void ValidateSchema_MalformedXml_ReturnsErrors()
    {
        var errors = SuisseTaxXsdValidator.ValidateSchema("<invalid></invalid>");
        Assert.NotEmpty(errors);
    }
}
