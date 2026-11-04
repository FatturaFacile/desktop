// <copyright file="InvoiceCalculationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FatturaFacile.Domain.Tests;

/// <summary>
/// Unit tests verifying invoice calculations, multi-currency totals, and Swiss 5-centimes rounding rules.
/// </summary>
public class InvoiceCalculationTests
{
    /// <summary>
    /// Verifies that standard EUR invoice correctly computes line items, subtotal, and VAT without fractional cent rounding.
    /// </summary>
    [Fact]
    public void Standard_EurInvoice_ComputesNetAndVatCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var invoice = Invoice.Create(
            new InvoiceCreateRequest(
                tenantId,
                contactId,
                InvoiceType.Standard,
                "INV-2026-0001",
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                "EUR",
                Jurisdiction.IT));

        // Act: 2 items: 10 * 100 EUR @ 22% VAT, 5 * 20 EUR with 10% discount @ 22% VAT
        invoice.AddItem(productId: null, "Consulting services", 10m, 100.00m, 22.00m, 0m);
        invoice.AddItem(
            productId: null,
            "Hosting plan",
            5m,
            20.00m,
            22.00m,
            10m); // 100 - 10 = 90.00

        // Assert
        // Item 1: Net = 1000.00, Tax = 220.00
        // Item 2: Net = 90.00, Tax = 19.80
        _ = invoice.TotalNet.Should().Be(1090.00m);
        _ = invoice.TotalTax.Should().Be(239.80m);
        _ = invoice.TotalGross.Should().Be(1329.80m);
        _ = invoice.RoundingAmount.Should().Be(0.00m);
        _ = invoice.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
    }

    /// <summary>
    /// Verifies that CHF invoices strictly apply the Swiss 0.05 rounding rule on total gross amounts.
    /// </summary>
    [Fact]
    public void Swiss_ChfInvoice_AppliesFiveCentimesRoundingRule()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var invoice = Invoice.Create(new InvoiceCreateRequest(
            tenantId,
            contactId,
            InvoiceType.ChQrBill,
            "INV-CH-2026-001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "CHF",
            Jurisdiction.CH));

        // Act: 1 item yielding fractional cents after VAT: 33.33 CHF @ 8.1% VAT = Net 33.33, Tax 2.70 => 36.03 CHF -> rounded to 36.05 CHF
        invoice.AddItem(productId: null, "Specialized Hardware Component", 1m, 33.33m, 8.1m, 0m);

        // Assert
        _ = invoice.TotalNet.Should().Be(33.33m);
        _ = invoice.TotalTax.Should().Be(2.70m);

        // 33.33 + 2.70 = 36.03 -> rounded to nearest 0.05 is 36.05
        _ = invoice.TotalGross.Should().Be(36.05m);
        _ = invoice.RoundingAmount.Should().Be(0.02m);
    }

    /// <summary>
    /// Verifies that recording payments transitions payment status from Unpaid to PartiallyPaid and Paid.
    /// </summary>
    [Fact]
    public void RegisterPayment_TransitionsStatusAccurately()
    {
        // Arrange
        var invoice = Invoice.Create(new InvoiceCreateRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            InvoiceType.Standard,
            "INV-2026-0002",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "EUR",
            Jurisdiction.IT));

        invoice.AddItem(productId: null, "Consulting", 1m, 100.00m, 0m); // Total: 100.00 EUR
        _ = invoice.PaymentStatus.Should().Be(PaymentStatus.Unpaid);

        // Act 1: Partial payment of 40 EUR
        invoice.RegisterPayment(40.00m, DateTimeOffset.UtcNow);

        // Assert 1
        _ = invoice.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);
        _ = invoice.TotalPaid.Should().Be(40.00m);

        // Act 2: Remaining 60 EUR
        invoice.RegisterPayment(60.00m, DateTimeOffset.UtcNow);

        // Assert 2
        _ = invoice.PaymentStatus.Should().Be(PaymentStatus.Paid);
        _ = invoice.TotalPaid.Should().Be(100.00m);
    }
}
