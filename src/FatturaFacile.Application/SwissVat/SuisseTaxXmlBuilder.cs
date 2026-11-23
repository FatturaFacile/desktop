// <copyright file="SuisseTaxXmlBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Serializes a <see cref="SwissVatCalculationResult"/> into eCH-0217 v2.0 SuisseTax XML.
/// </summary>
public static class SuisseTaxXmlBuilder
{
    /// <summary>
    /// Official eCH-0217 v2.0 XML namespace.
    /// </summary>
    public static readonly XNamespace ECh0217Ns = "http://www.ech.ch/xmlns/eCH-0217/2";

    /// <summary>
    /// Builds a formatted eCH-0217 v2.0 XML document for upload to SuisseTax.
    /// </summary>
    /// <param name="result">The calculated Swiss VAT return.</param>
    /// <param name="uidMwst">Company UID-MWST identifier.</param>
    /// <param name="companyName">Company legal name.</param>
    /// <param name="quadraturaPassed">Whether quadratura checks passed.</param>
    /// <returns>The UTF-8 XML declaration string.</returns>
    public static string Build(
        SwissVatCalculationResult result,
        string? uidMwst,
        string companyName,
        bool quadraturaPassed)
    {
        ArgumentNullException.ThrowIfNull(result);

        var methodWire = result.Method == SwissVatMethod.Saldosteuersatz ? "saldosteuersatz" : "effective";
        var basisWire = result.ConsiderationBasis == SwissVatConsiderationBasis.Received ? "received" : "agreed";
        var boxMap = result.Boxes.ToDictionary(static b => b.Code, static b => b, StringComparer.Ordinal);

        XElement BoxEl(string code)
        {
            var amount = boxMap.TryGetValue(code, out var b) ? b.Amount : 0m;
            return new XElement(
                ECh0217Ns + "box",
                new XAttribute("code", code),
                new XAttribute("amount", amount.ToString("0.00", CultureInfo.InvariantCulture)));
        }

        XElement RateBoxEl(AfcBoxEntry box) => new(
            ECh0217Ns + "rateBox",
            new XAttribute("code", box.Code),
            new XAttribute("taxableBase", (box.BaseAmount ?? 0m).ToString("0.00", CultureInfo.InvariantCulture)),
            new XAttribute("rate", (box.RatePercent ?? 0m).ToString("0.00", CultureInfo.InvariantCulture)),
            new XAttribute("taxAmount", (box.TaxAmount ?? 0m).ToString("0.00", CultureInfo.InvariantCulture)));

        XElement[] rateBoxes =
        [
            .. result.Boxes
                .Where(static b => string.Equals(b.Section, "tax_calculation", StringComparison.Ordinal) && !string.Equals(b.Code, "399", StringComparison.Ordinal))
                .Select(RateBoxEl),
        ];

        var root = new XElement(
            ECh0217Ns + "vatDeclaration",
            new XAttribute(XNamespace.Xmlns + "eCH-0217", ECh0217Ns.NamespaceName),
            new XAttribute("minorVersion", "0"),
            new XElement(
                ECh0217Ns + "header",
                new XElement(ECh0217Ns + "uid", SwissVatRateSchedule.NormalizeUid(uidMwst)),
                new XElement(ECh0217Ns + "companyName", string.IsNullOrWhiteSpace(companyName) ? "Swiss Taxpayer" : companyName),
                new XElement(ECh0217Ns + "settlementMethod", methodWire),
                new XElement(ECh0217Ns + "considerationBasis", basisWire),
                new XElement(ECh0217Ns + "periodFrom", result.PeriodFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                new XElement(ECh0217Ns + "periodTo", result.PeriodTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                new XElement(ECh0217Ns + "currency", "CHF"),
                new XElement(ECh0217Ns + "quadraturaPassed", quadraturaPassed ? "true" : "false")),
            new XElement(
                ECh0217Ns + "turnoverSection",
                BoxEl("200"),
                BoxEl("205"),
                BoxEl("220"),
                BoxEl("221"),
                BoxEl("225"),
                BoxEl("230"),
                BoxEl("235"),
                BoxEl("280"),
                BoxEl("289"),
                BoxEl("299")),
            new XElement(
                ECh0217Ns + "taxCalculationSection",
                rateBoxes,
                BoxEl("399")),
            new XElement(
                ECh0217Ns + "inputTaxSection",
                BoxEl("400"),
                BoxEl("405"),
                BoxEl("410"),
                BoxEl("415"),
                BoxEl("420"),
                BoxEl("479")),
            new XElement(
                ECh0217Ns + "settlementSection",
                BoxEl("500"),
                BoxEl("510")),
            new XElement(
                ECh0217Ns + "otherFlowsSection",
                BoxEl("900"),
                BoxEl("910")));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return doc.ToString();
    }
}
