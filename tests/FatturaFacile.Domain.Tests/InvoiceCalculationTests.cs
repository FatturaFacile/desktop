using FluentAssertions;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Enums;
using Xunit;

namespace FatturaFacile.Domain.Tests;

public class InvoiceCalculationTests
{
    [Fact]
    public void Standard_EurInvoice_ComputesNetAndVatCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var invoice = Invoice.Create(
            tenantId,
            contactId,
            InvoiceType.Standard,
            "INV-2026-0001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "EUR",
            Jurisdiction.IT);

        // Act: 2 items: 10 * 100 EUR @ 22% VAT, 5 * 20 EUR with 10% discount @ 22% VAT
        invoice.AddItem(null, "Consulting services", 10m, 100.00m, 22.00m, 0m);
        invoice.AddItem(null, "Hosting plan", 5m, 20.00m, 22.00m, 10m); // 100 - 10 = 90.00

        // Assert
        // Item 1: Net = 1000.00, Tax = 220.00
        // Item 2: Net = 90.00, Tax = 19.80
        invoice.TotalNet.Should().Be(1090.00m);
        invoice.TotalTax.Should().Be(239.80m);
        invoice.TotalGross.Should().Be(1329.80m);
        invoice.RoundingAmount.Should().Be(0.00m);
        invoice.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
    }

    [Fact]
    public void Swiss_ChfInvoice_AppliesFiveCentimesRoundingRule()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var invoice = Invoice.Create(
            tenantId,
            contactId,
            InvoiceType.ChQrBill,
            "INV-CH-2026-001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "CHF",
            Jurisdiction.CH);

        // Act: Net 101.33 + 8.1% CH VAT = 101.33 * 0.081 = 8.21. Unrounded = 109.54 CHF
        // 109.54 rounds to 109.55 CHF (nearest 0.05). Rounding difference = +0.01 CHF
        invoice.AddItem(null, "Swiss Services", 1m, 101.33m, 8.10m, 0m);

        // Assert
        invoice.TotalNet.Should().Be(101.33m);
        invoice.TotalTax.Should().Be(8.21m);
        invoice.TotalGross.Should().Be(109.55m);
        invoice.RoundingAmount.Should().Be(0.01m);
    }

    [Fact]
    public void Invoice_PaymentLifecycle_TransitionsStatusCorrectly()
    {
        // Arrange
        var invoice = Invoice.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            InvoiceType.Standard,
            "INV-PAY-001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            "EUR",
            Jurisdiction.IT);

        invoice.AddItem(null, "Product", 1m, 100m, 0m, 0m);

        // Act 1: Partial payment
        invoice.RegisterPayment(40m, DateTimeOffset.UtcNow);

        // Assert 1
        invoice.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);
        invoice.TotalPaid.Should().Be(40m);

        // Act 2: Complete payment
        invoice.RegisterPayment(60m, DateTimeOffset.UtcNow);

        // Assert 2
        invoice.PaymentStatus.Should().Be(PaymentStatus.Paid);
        invoice.TotalPaid.Should().Be(100m);

        // Act 3: Attempt cancel on fully paid invoice
        var cancelAction = () => invoice.Cancel("Customer requested cancel");
        cancelAction.Should().Throw<InvalidOperationException>()
            .WithMessage("*already fully paid*");
    }
}
