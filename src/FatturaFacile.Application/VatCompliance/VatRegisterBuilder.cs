// <copyright file="VatRegisterBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Builds the lines of the sales and purchases VAT registers from the issued invoices and the posted purchase invoices: the
/// sezionale of each document, the deductible and non-deductible VAT of purchases (pro-rata included) and the integration of
/// reverse-charge purchases, which also enter the sales register as autofatture.
/// </summary>
public static class VatRegisterBuilder
{
    /// <summary>
    /// The source type of a line built from a sales invoice.
    /// </summary>
    public const string InvoiceSource = "Invoice";

    /// <summary>
    /// The source type of a line built from a purchase invoice.
    /// </summary>
    public const string PurchaseSource = "PurchaseInvoice";

    /// <summary>
    /// The source type of a line entered by hand.
    /// </summary>
    public const string ManualSource = "Manual";

    /// <summary>
    /// The source type of the integration of a reverse-charge purchase in the sales register.
    /// </summary>
    public const string ReverseChargeSource = "PurchaseInvoiceIntegration";

    private const string DraftStatus = "draft";
    private const string PostedStatus = "Posted";
    private const string ReverseChargeNaturePrefix = "N6";
    private const decimal Hundred = 100m;

    private static readonly (string[] Markers, VatRegisterSection Section)[] SectionMarkers =
    [
        (["credit"], VatRegisterSection.CreditNote),
        (["debit"], VatRegisterSection.DebitNote),
        (["self", "autofattura"], VatRegisterSection.SelfInvoice),
        (["defer", "differit"], VatRegisterSection.Deferred),
    ];

    /// <summary>
    /// Gets the sezionale of a sales invoice from its type.
    /// </summary>
    /// <param name="invoiceType">The invoice type text.</param>
    /// <returns>The section.</returns>
    public static VatRegisterSection SectionOf(string? invoiceType)
    {
        var type = invoiceType ?? string.Empty;

        // The default of the tuple is the ordinary section.
        return SectionMarkers.FirstOrDefault(s => s.Markers.Any(m => type.Contains(m, StringComparison.OrdinalIgnoreCase))).Section;
    }

    /// <summary>
    /// Builds the unnumbered sales register lines of the invoices issued in a period; proforma and draft documents are skipped.
    /// </summary>
    /// <param name="invoices">The invoices.</param>
    /// <param name="itemsByInvoice">The items of the invoices by invoice identifier.</param>
    /// <param name="contacts">The contacts by identifier, to name the customers.</param>
    /// <returns>The lines, one per document and rate.</returns>
    public static IReadOnlyList<VatRegisterEntry> BuildSales(IReadOnlyList<Invoice> invoices, ILookup<Guid, InvoiceItem> itemsByInvoice, IReadOnlyDictionary<Guid, Contact> contacts)
    {
        ArgumentNullException.ThrowIfNull(invoices);
        ArgumentNullException.ThrowIfNull(itemsByInvoice);
        ArgumentNullException.ThrowIfNull(contacts);

        List<VatRegisterEntry> lines = [];
        foreach (var invoice in invoices.Where(static i => IsRegistrable(i)))
        {
            var section = SectionOf(invoice.InvoiceType);
            var sign = section == VatRegisterSection.CreditNote ? -1m : 1m;
            var contact = contacts.GetValueOrDefault(invoice.ContactId);

            foreach (var group in itemsByInvoice[invoice.Id].GroupBy(static i => (i.TaxRate, Nature: i.TaxRate == 0m ? i.VatNature : null)))
            {
                var taxable = group.Sum(static i => i.LineTotal);
                var vat = invoice.ReverseCharge ? 0m : Math.Round(taxable * group.Key.TaxRate / Hundred, 2, MidpointRounding.AwayFromZero);

                lines.Add(new VatRegisterEntry
                {
                    Kind = VatRegisterKind.Sales,
                    Section = section,
                    Year = invoice.Date.Year,
                    RegistrationDate = invoice.Date.Date,
                    DocumentDate = invoice.Date.Date,
                    DocumentNumber = invoice.InvoiceNumber,
                    PartnerName = NameOf(contact),
                    PartnerVatNumber = TaxIdOf(contact),
                    TaxableAmount = sign * taxable,
                    VatRate = group.Key.TaxRate,
                    VatAmount = sign * vat,
                    VatNature = group.Key.Nature,
                    IsReverseCharge = invoice.ReverseCharge,
                    IsSplitPayment = invoice.SplitPayment,
                    SourceType = InvoiceSource,
                    SourceId = invoice.Id,
                });
            }
        }

        return lines;
    }

    /// <summary>
    /// Builds the unnumbered purchases register lines of the purchase invoices registered in a period, with the deductible and
    /// non-deductible VAT, and the matching sales register lines of the reverse-charge purchases (autofatture).
    /// </summary>
    /// <param name="purchases">The purchase invoices.</param>
    /// <param name="linesByPurchase">The lines of the purchase invoices by purchase identifier.</param>
    /// <param name="contacts">The contacts by identifier, to name the suppliers.</param>
    /// <param name="proRataPercent">The pro-rata percentage of the year, applied to lines without their own deduction limit.</param>
    /// <returns>The purchases lines and the sales lines of the integrations.</returns>
    public static IReadOnlyList<VatRegisterEntry> BuildPurchases(
        IReadOnlyList<PurchaseInvoice> purchases,
        ILookup<Guid, PurchaseInvoiceLine> linesByPurchase,
        IReadOnlyDictionary<Guid, Contact> contacts,
        decimal proRataPercent)
    {
        ArgumentNullException.ThrowIfNull(purchases);
        ArgumentNullException.ThrowIfNull(linesByPurchase);
        ArgumentNullException.ThrowIfNull(contacts);

        List<VatRegisterEntry> lines = [];
        foreach (var purchase in purchases.Where(static p => string.Equals(p.Status, PostedStatus, StringComparison.Ordinal)))
        {
            var contact = contacts.GetValueOrDefault(purchase.SupplierId);
            var documentLimit = DocumentLimitOf(purchase);
            var documentLines = linesByPurchase[purchase.Id].ToList();

            if (documentLines.Count == 0)
            {
                var rate = purchase.TotalNet > 0m ? Math.Round(purchase.TotalVat / purchase.TotalNet * Hundred, 2, MidpointRounding.AwayFromZero) : 0m;
                documentLines.Add(new PurchaseInvoiceLine { NetAmount = purchase.TotalNet, VatRate = rate, VatAmount = purchase.TotalVat });
            }

            foreach (var group in documentLines.GroupBy(static l => (l.VatRate, Nature: l.VatRate == 0m || IsReverseChargeNature(l.VatNature) ? l.VatNature : null)))
            {
                var taxable = group.Sum(static l => l.NetAmount);
                var reverseCharge = IsReverseChargeNature(group.Key.Nature);
                var vat = reverseCharge
                    ? Math.Round(taxable * ReceiptsVentilation.OrdinaryRate / Hundred, 2, MidpointRounding.AwayFromZero)
                    : group.Sum(static l => l.VatAmount);
                var (deductible, nonDeductible) = ProRataCalculator.Split(vat, documentLimit ?? DeductiblePercentOf(group, proRataPercent));

                var line = NewLine(purchase, contact, PurchaseSource, VatRegisterKind.Purchases, VatRegisterSection.Ordinary);
                line.VatRate = group.Key.VatRate;
                line.VatNature = group.Key.Nature;
                line.TaxableAmount = taxable;
                line.VatAmount = vat;
                line.DeductibleVat = deductible;
                line.NonDeductibleVat = nonDeductible;
                line.IsReverseCharge = reverseCharge;
                lines.Add(line);

                if (reverseCharge)
                {
                    // The integration is registered a second time in the sales register, with the full VAT, as an autofattura.
                    var integration = NewLine(purchase, contact, ReverseChargeSource, VatRegisterKind.Sales, VatRegisterSection.SelfInvoice);
                    integration.VatRate = ReceiptsVentilation.OrdinaryRate;
                    integration.VatNature = group.Key.Nature;
                    integration.TaxableAmount = taxable;
                    integration.VatAmount = vat;
                    integration.IsReverseCharge = true;
                    lines.Add(integration);
                }
            }
        }

        return lines;
    }

    private static decimal? DocumentLimitOf(PurchaseInvoice purchase)
    {
        // A document whose deductible VAT is already below its VAT carries its own limit (cars, representation costs).
        var limited = purchase.TotalVat > 0m && purchase.DeductibleVat < purchase.TotalVat;

        return limited ? Math.Round(purchase.DeductibleVat / purchase.TotalVat * Hundred, 4, MidpointRounding.AwayFromZero) : null;
    }

    private static decimal DeductiblePercentOf(IEnumerable<PurchaseInvoiceLine> group, decimal proRataPercent)
    {
        var limited = group.Where(static l => l.DeductiblePercent < Hundred).Select(static l => l.DeductiblePercent).ToList();

        return limited.Count > 0 ? limited.Min() : proRataPercent;
    }

    private static VatRegisterEntry NewLine(PurchaseInvoice purchase, Contact? contact, string sourceType, VatRegisterKind kind, VatRegisterSection section) => new()
    {
        Kind = kind,
        Section = section,
        Year = purchase.RegistrationDate.Year,
        RegistrationDate = purchase.RegistrationDate.Date,
        DocumentDate = purchase.DocumentDate.Date,
        DocumentNumber = purchase.DocumentNumber,
        PartnerName = NameOf(contact),
        PartnerVatNumber = TaxIdOf(contact),
        SourceType = sourceType,
        SourceId = purchase.Id,
    };

    private static bool IsRegistrable(Invoice invoice) =>
        !string.Equals(invoice.Status, DraftStatus, StringComparison.OrdinalIgnoreCase)
        && !invoice.InvoiceType.Contains("proforma", StringComparison.OrdinalIgnoreCase);

    private static bool IsReverseChargeNature(string? nature) => nature?.StartsWith(ReverseChargeNaturePrefix, StringComparison.Ordinal) == true;

    private static string? NameOf(Contact? contact) => contact is null ? null : DisplayName(contact);

    private static string DisplayName(Contact contact) =>
        string.IsNullOrWhiteSpace(contact.CompanyName) ? $"{contact.FirstName} {contact.LastName}".Trim() : contact.CompanyName;

    private static string? TaxIdOf(Contact? contact) => string.IsNullOrWhiteSpace(contact?.VatNumber) ? contact?.FiscalCode : contact.VatNumber;
}
