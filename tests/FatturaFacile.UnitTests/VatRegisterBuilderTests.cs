// <copyright file="VatRegisterBuilderTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests that the VAT register lines are built from invoices and purchase invoices with the right sezionale, sign, deductible
/// VAT (pro-rata included) and reverse-charge integration.
/// </summary>
public sealed class VatRegisterBuilderTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid SupplierId = Guid.NewGuid();
    private static readonly DateTime Day = new(2027, 3, 10, 0, 0, 0, DateTimeKind.Unspecified);

    private static readonly Dictionary<Guid, Contact> Contacts = new()
    {
        [CustomerId] = new Contact { Id = CustomerId, CompanyName = "Rossi Srl", VatNumber = "01234567897" },
        [SupplierId] = new Contact { Id = SupplierId, FirstName = "Anna", LastName = "Bianchi", FiscalCode = "BNCNNA80A41H501Z" },
    };

    /// <summary>
    /// The sezionale follows the type text of the invoice.
    /// </summary>
    /// <param name="type">The invoice type text.</param>
    /// <param name="expected">The expected section.</param>
    [Theory]
    [InlineData("standard", VatRegisterSection.Ordinary)]
    [InlineData("credit_note", VatRegisterSection.CreditNote)]
    [InlineData("debit_note", VatRegisterSection.DebitNote)]
    [InlineData("deferred", VatRegisterSection.Deferred)]
    [InlineData("fattura_differita", VatRegisterSection.Deferred)]
    [InlineData("self_invoice", VatRegisterSection.SelfInvoice)]
    [InlineData("autofattura", VatRegisterSection.SelfInvoice)]
    [InlineData(null, VatRegisterSection.Ordinary)]
    public void SectionOf_MapsTheInvoiceType(string? type, VatRegisterSection expected) =>
        Assert.Equal(expected, VatRegisterBuilder.SectionOf(type));

    /// <summary>
    /// A sales invoice yields one line per rate, with the customer named, and the zero-rate line keeps its nature.
    /// </summary>
    [Fact]
    public void BuildSales_GroupsTheItemsByRateAndKeepsTheNature()
    {
        var invoice = Invoice("standard");
        var items = Items(invoice, (100m, 22m, null), (50m, 22m, null), (40m, 0m, "N4"));

        var lines = VatRegisterBuilder.BuildSales([invoice], items.ToLookup(static i => i.InvoiceId), Contacts);

        Assert.Equal(2, lines.Count);
        var ordinary = Assert.Single(lines, static l => l.VatRate == 22m);
        Assert.Equal(150m, ordinary.TaxableAmount);
        Assert.Equal(33m, ordinary.VatAmount);
        Assert.Equal("Rossi Srl", ordinary.PartnerName);
        Assert.Equal("01234567897", ordinary.PartnerVatNumber);
        Assert.Equal(VatRegisterKind.Sales, ordinary.Kind);
        var exempt = Assert.Single(lines, static l => l.VatRate == 0m);
        Assert.Equal("N4", exempt.VatNature);
        Assert.Equal(0m, exempt.VatAmount);
    }

    /// <summary>
    /// A credit note goes to its own sezionale with negative amounts.
    /// </summary>
    [Fact]
    public void BuildSales_CreditNote_IsNegativeInItsOwnSection()
    {
        var invoice = Invoice("credit_note");
        var items = Items(invoice, (100m, 22m, null));

        var line = Assert.Single(VatRegisterBuilder.BuildSales([invoice], items.ToLookup(static i => i.InvoiceId), Contacts));

        Assert.Equal(VatRegisterSection.CreditNote, line.Section);
        Assert.Equal(-100m, line.TaxableAmount);
        Assert.Equal(-22m, line.VatAmount);
    }

    /// <summary>
    /// Draft and proforma documents are never registered.
    /// </summary>
    [Fact]
    public void BuildSales_SkipsDraftAndProforma()
    {
        var draft = Invoice("standard", "draft");
        var proforma = Invoice("proforma");
        var items = Items(draft, (100m, 22m, null)).Concat(Items(proforma, (100m, 22m, null)));

        Assert.Empty(VatRegisterBuilder.BuildSales([draft, proforma], items.ToLookup(static i => i.InvoiceId), Contacts));
    }

    /// <summary>
    /// A reverse-charge sale is registered without VAT and flagged.
    /// </summary>
    [Fact]
    public void BuildSales_ReverseCharge_HasNoVat()
    {
        var invoice = Invoice("standard");
        invoice.ReverseCharge = true;
        var items = Items(invoice, (100m, 22m, null));

        var line = Assert.Single(VatRegisterBuilder.BuildSales([invoice], items.ToLookup(static i => i.InvoiceId), Contacts));

        Assert.Equal(0m, line.VatAmount);
        Assert.True(line.IsReverseCharge);
    }

    /// <summary>
    /// The VAT of a purchase is split by the pro-rata percentage into deductible and non-deductible VAT.
    /// </summary>
    [Fact]
    public void BuildPurchases_AppliesTheProRata()
    {
        var purchase = Purchase(totalNet: 1000m, totalVat: 220m, deductible: 220m);
        var lines = new[] { new PurchaseInvoiceLine { PurchaseInvoiceId = purchase.Id, NetAmount = 1000m, VatRate = 22m, VatAmount = 220m } };

        var line = Assert.Single(VatRegisterBuilder.BuildPurchases([purchase], lines.ToLookup(static l => l.PurchaseInvoiceId), Contacts, proRataPercent: 60m));

        Assert.Equal(132m, line.DeductibleVat);
        Assert.Equal(88m, line.NonDeductibleVat);
        Assert.Equal(220m, line.VatAmount);
        Assert.Equal("Anna Bianchi", line.PartnerName);
    }

    /// <summary>
    /// A document whose deductible VAT is already below its VAT keeps its own limit instead of the pro-rata.
    /// </summary>
    [Fact]
    public void BuildPurchases_DocumentLimit_WinsOverTheProRata()
    {
        var purchase = Purchase(totalNet: 1000m, totalVat: 220m, deductible: 88m);
        var lines = new[] { new PurchaseInvoiceLine { PurchaseInvoiceId = purchase.Id, NetAmount = 1000m, VatRate = 22m, VatAmount = 220m } };

        var line = Assert.Single(VatRegisterBuilder.BuildPurchases([purchase], lines.ToLookup(static l => l.PurchaseInvoiceId), Contacts, proRataPercent: 60m));

        Assert.Equal(88m, line.DeductibleVat);
        Assert.Equal(132m, line.NonDeductibleVat);
    }

    /// <summary>
    /// A purchase without lines is registered from its document totals.
    /// </summary>
    [Fact]
    public void BuildPurchases_WithoutLines_UsesTheDocumentTotals()
    {
        var purchase = Purchase(totalNet: 500m, totalVat: 110m, deductible: 110m);

        var line = Assert.Single(VatRegisterBuilder.BuildPurchases([purchase], Array.Empty<PurchaseInvoiceLine>().ToLookup(static l => l.PurchaseInvoiceId), Contacts, proRataPercent: 100m));

        Assert.Equal(22m, line.VatRate);
        Assert.Equal(500m, line.TaxableAmount);
        Assert.Equal(110m, line.DeductibleVat);
    }

    /// <summary>
    /// A reverse-charge purchase is registered in the purchases register and integrated in the sales register as an autofattura.
    /// </summary>
    [Fact]
    public void BuildPurchases_ReverseCharge_AddsTheSelfInvoiceToTheSalesRegister()
    {
        var purchase = Purchase(totalNet: 1000m, totalVat: 0m, deductible: 0m);
        var lines = new[] { new PurchaseInvoiceLine { PurchaseInvoiceId = purchase.Id, NetAmount = 1000m, VatRate = 0m, VatAmount = 0m, VatNature = "N6.3" } };

        var result = VatRegisterBuilder.BuildPurchases([purchase], lines.ToLookup(static l => l.PurchaseInvoiceId), Contacts, proRataPercent: 100m);

        Assert.Equal(2, result.Count);
        var bought = Assert.Single(result, static l => l.Kind == VatRegisterKind.Purchases);
        Assert.Equal(220m, bought.VatAmount);
        Assert.Equal(220m, bought.DeductibleVat);
        var integration = Assert.Single(result, static l => l.Kind == VatRegisterKind.Sales);
        Assert.Equal(VatRegisterSection.SelfInvoice, integration.Section);
        Assert.Equal(220m, integration.VatAmount);
        Assert.Equal(VatRegisterBuilder.ReverseChargeSource, integration.SourceType);
    }

    /// <summary>
    /// A purchase that is not posted is never registered.
    /// </summary>
    [Fact]
    public void BuildPurchases_SkipsDraftPurchases()
    {
        var purchase = Purchase(totalNet: 100m, totalVat: 22m, deductible: 22m);
        purchase.Status = "Draft";

        Assert.Empty(VatRegisterBuilder.BuildPurchases([purchase], Array.Empty<PurchaseInvoiceLine>().ToLookup(static l => l.PurchaseInvoiceId), Contacts, proRataPercent: 100m));
    }

    private static Invoice Invoice(string type, string status = "issued") => new()
    {
        ContactId = CustomerId,
        InvoiceType = type,
        InvoiceNumber = "2027/10",
        Date = Day,
        Status = status,
    };

    private static List<InvoiceItem> Items(Invoice invoice, params (decimal Total, decimal Rate, string? Nature)[] specs) =>
        [.. specs.Select(i => new InvoiceItem { InvoiceId = invoice.Id, LineTotal = i.Total, TaxRate = i.Rate, VatNature = i.Nature })];

    private static PurchaseInvoice Purchase(decimal totalNet, decimal totalVat, decimal deductible) => new()
    {
        SupplierId = SupplierId,
        DocumentNumber = "F-77",
        DocumentDate = Day,
        RegistrationDate = Day,
        TotalNet = totalNet,
        TotalVat = totalVat,
        DeductibleVat = deductible,
        Status = "Posted",
    };
}
