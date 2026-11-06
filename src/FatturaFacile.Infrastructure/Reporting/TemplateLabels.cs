// <copyright file="TemplateLabels.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// The fixed texts written in the standard templates, per language. Users change them directly in Word.
/// </summary>
/// <param name="Customer">"Customer" heading.</param>
/// <param name="VatNumber">VAT / UID number label.</param>
/// <param name="Number">Document number label.</param>
/// <param name="Date">Date label.</param>
/// <param name="DueDate">Due date label.</param>
/// <param name="Columns">Item table headings: description, quantity, unit, price, discount, VAT, amount.</param>
/// <param name="VatSummary">VAT summary heading and columns: title, rate, nature, taxable, tax.</param>
/// <param name="Payment">Payment label.</param>
/// <param name="Notes">Notes label.</param>
/// <param name="Page">Page label.</param>
public sealed record TemplateLabels(
    string Customer,
    string VatNumber,
    string Number,
    string Date,
    string DueDate,
    IReadOnlyList<string> Columns,
    IReadOnlyList<string> VatSummary,
    string Payment,
    string Notes,
    string Page)
{
    /// <summary>
    /// Gets the Italian labels.
    /// </summary>
    public static TemplateLabels Italian { get; } = new(
        "Spett.le",
        "P. IVA / IDI",
        "Numero",
        "Data",
        "Scadenza",
        ["Descrizione", "Q.tà", "U.M.", "Prezzo", "Sconto", "IVA", "Importo"],
        ["Riepilogo IVA", "Aliquota", "Natura", "Imponibile", "Imposta"],
        "Pagamento",
        "Note",
        "Pagina");

    /// <summary>
    /// Gets the English labels.
    /// </summary>
    public static TemplateLabels English { get; } = new(
        "Bill to",
        "VAT / UID no.",
        "Number",
        "Date",
        "Due date",
        ["Description", "Qty", "Unit", "Price", "Discount", "VAT", "Amount"],
        ["VAT summary", "Rate", "Exemption", "Taxable", "VAT"],
        "Payment",
        "Notes",
        "Page");

    /// <summary>
    /// Gets the German labels.
    /// </summary>
    public static TemplateLabels German { get; } = new(
        "Rechnungsempfänger",
        "MWST-Nr. / UID",
        "Nummer",
        "Datum",
        "Fällig am",
        ["Beschreibung", "Menge", "Einheit", "Preis", "Rabatt", "MWST", "Betrag"],
        ["MWST-Zusammenfassung", "Satz", "Grund", "Netto", "MWST"],
        "Zahlung",
        "Bemerkungen",
        "Seite");

    /// <summary>
    /// Gets the French labels.
    /// </summary>
    public static TemplateLabels French { get; } = new(
        "Destinataire",
        "N° TVA / IDE",
        "Numéro",
        "Date",
        "Échéance",
        ["Description", "Qté", "Unité", "Prix", "Rabais", "TVA", "Montant"],
        ["Récapitulatif TVA", "Taux", "Motif", "Montant net", "TVA"],
        "Paiement",
        "Remarques",
        "Page");
}
