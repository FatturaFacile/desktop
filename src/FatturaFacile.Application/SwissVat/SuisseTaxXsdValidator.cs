// <copyright file="SuisseTaxXsdValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Validates SuisseTax eCH-0217 v2.0 XML documents against the embedded XSD schema and business rules.
/// </summary>
public static class SuisseTaxXsdValidator
{
    private const string SchemaResourceName = "FatturaFacile.Application.SwissVat.suissetax-ech0217-v2.xsd";

    /// <summary>
    /// Validates <paramref name="xml"/> against the eCH-0217 XSD schema only.
    /// </summary>
    /// <param name="xml">XML payload string.</param>
    /// <returns>List of XSD schema validation errors, empty when valid.</returns>
    public static IReadOnlyList<string> ValidateSchema(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return ["SuisseTax XML payload is empty."];
        }

        List<string> errors = [];
        try
        {
            var doc = XDocument.Parse(xml);
            var schemas = new XmlSchemaSet();
            using var stream = typeof(SuisseTaxXsdValidator).Assembly.GetManifestResourceStream(SchemaResourceName)
                ?? throw new InvalidOperationException($"Missing embedded schema resource '{SchemaResourceName}'.");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
            _ = schemas.Add(SuisseTaxXmlBuilder.ECh0217Ns.NamespaceName, reader);

            doc.Validate(schemas, (_, args) => errors.Add(args.Message));
        }
        catch (XmlException ex)
        {
            errors.Add(ex.Message);
        }

        return errors;
    }

    /// <summary>
    /// Performs full pre-export validation: XSD schema, UID-MWST validity, and quadratura gate.
    /// </summary>
    /// <param name="xml">Generated XML payload.</param>
    /// <param name="uidMwst">Configured UID-MWST.</param>
    /// <param name="quadratura">Quadratura report.</param>
    /// <returns>Structured validation result.</returns>
    public static SuisseTaxValidationResultDto ValidateAll(
        string xml,
        string? uidMwst,
        QuadraturaReportDto quadratura)
    {
        ArgumentNullException.ThrowIfNull(quadratura);

        var xsdErrors = ValidateSchema(xml);
        List<string> errors = [.. xsdErrors];

        if (!SwissVatRateSchedule.IsValidUidMwst(uidMwst))
        {
            errors.Add("UID-MWST must be configured in CHE-XXX.XXX.XXX MWST format with valid Mod-11 check digit.");
        }

        if (!quadratura.Passed)
        {
            foreach (var blocker in quadratura.Checks.Where(static c => !c.Passed && string.Equals(c.Severity, "blocker", StringComparison.Ordinal)))
            {
                errors.Add($"[{blocker.Code}] Expected {blocker.Expected}, actual {blocker.Actual}.");
            }
        }

        return new SuisseTaxValidationResultDto(
            Valid: errors.Count == 0,
            XsdValid: xsdErrors.Count == 0,
            QuadraturaPassed: quadratura.Passed,
            SchemaVersion: "eCH-0217-v2.0",
            Errors: errors);
    }
}
