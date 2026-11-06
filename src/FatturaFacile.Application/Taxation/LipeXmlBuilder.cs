// <copyright file="LipeXmlBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FatturaFacile.Application.Taxation;

/// <summary>
/// Writes the XML file of the Comunicazione liquidazioni periodiche IVA (supply IVP18, namespace
/// urn:www.agenziaentrate.gov.it:specificheTecniche:sco:ivp). The file must be checked with the control software of the
/// Agenzia delle Entrate before it is sent (directly or through an intermediary).
/// </summary>
public static class LipeXmlBuilder
{
    private static readonly XNamespace Iv = "urn:www.agenziaentrate.gov.it:specificheTecniche:sco:ivp";

    /// <summary>
    /// Gets the file name (IT + fiscal code + _LI_ + five character progressive).
    /// </summary>
    /// <param name="data">The communication data.</param>
    /// <returns>The file name.</returns>
    public static string FileName(LipeData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var period = data.Month is { } month
            ? string.Create(CultureInfo.InvariantCulture, $"M{month:00}")
            : string.Create(CultureInfo.InvariantCulture, $"T{data.Quarter ?? 1:00}");
        return string.Create(CultureInfo.InvariantCulture, $"IT{data.TaxCode}_LI_{data.Year % 100:00}{period}.xml");
    }

    /// <summary>
    /// Builds the XML document.
    /// </summary>
    /// <param name="data">The communication data.</param>
    /// <returns>The UTF-8 encoded document.</returns>
    public static byte[] Build(LipeData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        XElement header = new(Iv + "Intestazione", new XElement(Iv + "CodiceFornitura", "IVP18"));
        XElement front = new(
            Iv + "Frontespizio",
            new XElement(Iv + "CodiceFiscale", data.TaxCode),
            new XElement(Iv + "AnnoImposta", data.Year.ToString(CultureInfo.InvariantCulture)),
            new XElement(Iv + "PartitaIVA", data.VatNumber));

        if (!string.IsNullOrWhiteSpace(data.DeclarantTaxCode))
        {
            header.Add(new XElement(Iv + "CodiceFiscaleDichiarante", data.DeclarantTaxCode), new XElement(Iv + "CodiceCarica", data.DeclarantRoleCode));
            front.Add(new XElement(Iv + "CFDichiarante", data.DeclarantTaxCode), new XElement(Iv + "CodiceCaricaDichiarante", data.DeclarantRoleCode));
        }

        front.Add(new XElement(Iv + "FirmaDichiarazione", "1"));

        XElement module = new(Iv + "Modulo", new XElement(Iv + "NumeroModulo", "1"));
        module.Add(data.Month is { } month
            ? new XElement(Iv + "Mese", month.ToString(CultureInfo.InvariantCulture))
            : new XElement(Iv + "Trimestre", (data.Quarter ?? 1).ToString(CultureInfo.InvariantCulture)));
        AddAmount(module, "TotaleOperazioniAttive", data.ActiveOperations);
        AddAmount(module, "TotaleOperazioniPassive", data.PassiveOperations);
        AddAmount(module, "IvaEsigibile", data.OutputVat);
        AddAmount(module, "IvaDetratta", data.InputVat);
        AddAmount(module, "IvaDovuta", data.OutputVat - data.InputVat);
        AddAmount(module, "IvaCredito", data.InputVat - data.OutputVat);
        AddAmount(module, "CreditoPeriodoPrecedente", data.PreviousCredit);
        AddAmount(module, "InteressiDovuti", data.Interest);
        AddAmount(module, "ImportoDaVersare", data.Payable);
        AddAmount(module, "ImportoACredito", -data.Payable);

        XElement root = new(
            Iv + "Fornitura",
            new XAttribute(XNamespace.Xmlns + "iv", Iv.NamespaceName),
            header,
            new XElement(Iv + "Comunicazione", new XAttribute("identificativo", "00001"), front, new XElement(Iv + "DatiContabili", module)));

        using var stream = new MemoryStream();
        XmlWriterSettings settings = new() { Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), Indent = true };

        using (var writer = XmlWriter.Create(stream, settings))
        {
            new XDocument(new XDeclaration("1.0", "UTF-8", standalone: null), root).Save(writer);
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Formats an amount as required by the specifications (comma as decimal separator, two decimals).
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <returns>The formatted amount.</returns>
    public static string FormatAmount(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

    private static void AddAmount(XElement module, string name, decimal value)
    {
        if (value > 0m)
        {
            module.Add(new XElement(Iv + name, FormatAmount(value)));
        }
    }
}
