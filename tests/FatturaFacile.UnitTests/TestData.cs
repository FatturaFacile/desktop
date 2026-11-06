// <copyright file="TestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Factory of consistent CH / IT sample entities for compliance tests.
/// </summary>
internal static class TestData
{
    /// <summary>
    /// Creates a Swiss tenant with structured address.
    /// </summary>
    /// <param name="iban">The IBAN.</param>
    /// <param name="qrIban">The optional QR-IBAN.</param>
    /// <returns>The tenant.</returns>
    public static Tenant SwissTenant(string? iban = "CH9300762011623852957", string? qrIban = null)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = "FatturaFacile Schweiz AG",
            LegalName = "FatturaFacile Schweiz AG",
            Country = "CH",
            DefaultCurrency = "CHF",
            Iban = iban,
            QrIban = qrIban,
            Street = "Bahnhofstrasse",
            HouseNumber = "1",
            PostalCode = "8001",
            City = "Zürich",
        };
    }

    /// <summary>
    /// Creates a Swiss customer with structured address.
    /// </summary>
    /// <returns>The contact.</returns>
    public static Contact SwissContact()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            CompanyName = "Muster AG",
            Country = "CH",
            Street = "Via Nassa",
            HouseNumber = "5",
            PostalCode = "6900",
            City = "Lugano",
        };
    }

    /// <summary>
    /// Creates an Italian tenant with structured address.
    /// </summary>
    /// <returns>The tenant.</returns>
    public static Tenant ItalianTenant()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = "FatturaFacile Italia S.r.l.",
            LegalName = "FatturaFacile Italia S.r.l.",
            Country = "IT",
            DefaultCurrency = "EUR",
            VatNumber = "IT01234567890",
            FiscalCode = "01234567890",
            Iban = "IT60X0542811101000000123456",
            Street = "Via Roma",
            HouseNumber = "10",
            PostalCode = "20121",
            City = "Milano",
            Province = "MI",
            TaxRegime = "RF01",
        };
    }

    /// <summary>
    /// Creates an Italian B2B customer.
    /// </summary>
    /// <returns>The contact.</returns>
    public static Contact ItalianContact()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            CompanyName = "Rossi S.r.l.",
            VatNumber = "IT09876543210",
            FiscalCode = "09876543210",
            SdiCode = "M5UXCR1",
            Country = "IT",
            Street = "Corso Vittorio Emanuele II",
            HouseNumber = "45",
            PostalCode = "10121",
            City = "Torino",
            Province = "TO",
        };
    }

    /// <summary>
    /// Creates an invoice with the given lines.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="items">The invoice lines.</param>
    /// <returns>The invoice with totals computed from the lines.</returns>
    public static Invoice Invoice(Guid tenantId, string currency, params InvoiceItem[] items)
    {
        Invoice invoice = new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceNumber = "FAT-2026-001",
            Date = new DateTime(2026, 11, 5, 0, 0, 0, DateTimeKind.Unspecified),
            DueDate = new DateTime(2026, 12, 5, 0, 0, 0, DateTimeKind.Unspecified),
            Currency = currency,
            SequentialNumber = 1,
        };

        foreach (var item in items)
        {
            invoice.Items.Add(item);
        }

        invoice.TotalNet = items.Sum(static i => i.LineTotal);
        invoice.TotalTax = items.Sum(static i => Math.Round(i.LineTotal * i.TaxRate / 100m, 2, MidpointRounding.AwayFromZero));
        invoice.TotalGross = invoice.TotalNet + invoice.TotalTax;
        return invoice;
    }

    /// <summary>
    /// Creates an invoice line.
    /// </summary>
    /// <param name="description">The description.</param>
    /// <param name="quantity">The quantity.</param>
    /// <param name="unitPrice">The unit price.</param>
    /// <param name="taxRate">The VAT rate.</param>
    /// <param name="nature">The optional VAT nature.</param>
    /// <returns>The line.</returns>
    public static InvoiceItem Line(string description, decimal quantity, decimal unitPrice, decimal taxRate, string? nature = null)
    {
        return new()
        {
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            VatNature = nature,
            LineTotal = Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero),
            Uom = "PZ",
        };
    }
}
