// <copyright file="FatturaPaPurchaseParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Purchasing;

/// <summary>
/// Reads received FatturaPA invoices (passive cycle) into supplier invoices.
/// </summary>
public static class FatturaPaPurchaseParser
{
    private static readonly string[] CreditNoteTypes = ["TD04", "TD08"];

    /// <summary>
    /// Reads every invoice body of a FatturaPA file.
    /// </summary>
    /// <param name="xml">The XML document (already extracted from a P7M envelope).</param>
    /// <returns>The invoices.</returns>
    /// <exception cref="InvalidDataException">The document is not a FatturaPA invoice.</exception>
    public static IReadOnlyList<ParsedPurchaseInvoice> Parse(byte[] xml)
    {
        ArgumentNullException.ThrowIfNull(xml);

        using var stream = new MemoryStream(xml);
        var root = XDocument.Load(stream).Root;

        if (root is null || !string.Equals(root.Name.LocalName, "FatturaElettronica", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Il file non è una fattura elettronica FatturaPA.");
        }

        var seller = Child(Child(root, "FatturaElettronicaHeader"), "CedentePrestatore");
        List<ParsedPurchaseInvoice> invoices = [];

        foreach (var body in Children(root, "FatturaElettronicaBody"))
        {
            invoices.Add(ReadBody(body, seller));
        }

        return invoices;
    }

    private static ParsedPurchaseInvoice ReadBody(XElement body, XElement? seller)
    {
        var general = Child(Child(body, "DatiGenerali"), "DatiGeneraliDocumento");
        var type = Text(general, "TipoDocumento") ?? "TD01";
        var sign = CreditNoteTypes.Contains(type, StringComparer.Ordinal) ? -1m : 1m;
        var payment = Child(Child(body, "DatiPagamento"), "DettaglioPagamento");

        ParsedPurchaseInvoice parsed = new()
        {
            Supplier = ReadSupplier(seller),
            DocumentType = type,
            Invoice = new PurchaseInvoice
            {
                DocumentNumber = Text(general, "Numero") ?? string.Empty,
                DocumentDate = Date(Text(general, "Data")) ?? DateTime.Today,
                RegistrationDate = DateTime.Today,
                DueDate = Date(Text(payment, "DataScadenzaPagamento")),
                Currency = Text(general, "Divisa") ?? "EUR",
                PaymentIban = Text(payment, "IBAN"),
                Status = "Draft",
            },
        };

        var goods = Child(body, "DatiBeniServizi");

        foreach (var line in Children(goods, "DettaglioLinee"))
        {
            AddLine(parsed, Text(line, "Descrizione"), sign * Amount(Text(line, "PrezzoTotale")), Amount(Text(line, "AliquotaIVA")), Text(line, "Natura"));
        }

        if (parsed.Lines.Count == 0)
        {
            foreach (var summary in Children(goods, "DatiRiepilogo"))
            {
                AddLine(parsed, "Riepilogo IVA", sign * Amount(Text(summary, "ImponibileImporto")), Amount(Text(summary, "AliquotaIVA")), Text(summary, "Natura"));
            }
        }

        return parsed;
    }

    private static void AddLine(ParsedPurchaseInvoice parsed, string? description, decimal net, decimal rate, string? nature) =>
        parsed.Lines.Add(new PurchaseInvoiceLine
        {
            Description = string.IsNullOrWhiteSpace(description) ? "-" : description.Trim(),
            NetAmount = net,
            VatRate = rate,
            VatAmount = Math.Round(net * rate / 100m, 2, MidpointRounding.AwayFromZero),
            DeductiblePercent = 100m,
            VatNature = nature,
        });

    private static Contact ReadSupplier(XElement? seller)
    {
        var data = Child(seller, "DatiAnagrafici");
        var vat = Child(data, "IdFiscaleIVA");
        var registry = Child(data, "Anagrafica");
        var office = Child(seller, "Sede");

        return new Contact
        {
            Type = "supplier",
            Country = Text(vat, "IdPaese") ?? Text(office, "Nazione") ?? "IT",
            VatNumber = Text(vat, "IdCodice"),
            FiscalCode = Text(data, "CodiceFiscale"),
            CompanyName = Text(registry, "Denominazione"),
            FirstName = Text(registry, "Nome"),
            LastName = Text(registry, "Cognome"),
            Street = Text(office, "Indirizzo"),
            HouseNumber = Text(office, "NumeroCivico"),
            PostalCode = Text(office, "CAP"),
            City = Text(office, "Comune"),
            Province = Text(office, "Provincia"),
        };
    }

    private static XElement? Child(XElement? parent, string localName) =>
        parent?.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, localName, StringComparison.Ordinal));

    private static IEnumerable<XElement> Children(XElement? parent, string localName) =>
        parent?.Elements().Where(e => string.Equals(e.Name.LocalName, localName, StringComparison.Ordinal)) ?? [];

    private static string? Text(XElement? parent, string localName) =>
        Child(parent, localName)?.Value.Trim() is { Length: > 0 } value ? value : null;

    private static decimal Amount(string? text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    private static DateTime? Date(string? text) =>
        DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null;
}
