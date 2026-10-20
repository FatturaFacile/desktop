using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Application.UnitTests;

public class DomainValueObjectTests
{
    [Fact]
    public void FiscalCode_ValidCode_ShouldInstantiate()
    {
        // Act
        var fc = new FiscalCode("RSSMRA85M01H501Z");

        // Assert
        _ = fc.Value.Should().Be("RSSMRA85M01H501Z");
        _ = fc.ToString().Should().Be("RSSMRA85M01H501Z");
    }

    [Fact]
    public void FiscalCode_InvalidCode_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new FiscalCode("INVALID_CF");

        // Assert
        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid Italian Fiscal Code*");
    }

    [Fact]
    public void VatNumber_ValidItalianVat_ShouldInstantiate()
    {
        // Act
        var vat = new VatNumber("12345678901", "IT");

        // Assert
        _ = vat.Value.Should().Be("12345678901");
        _ = vat.CountryCode.Should().Be("IT");
    }

    [Fact]
    public void VatNumber_ValidSwissVat_ShouldInstantiate()
    {
        // Act
        var vat = new VatNumber("CHE-123.456.789 MWST", "CH");

        // Assert
        _ = vat.Value.Should().Be("CHE123456789MWST");
        _ = vat.CountryCode.Should().Be("CH");
    }

    [Fact]
    public void VatNumber_InvalidItalianVat_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new VatNumber("123", "IT");

        // Assert
        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid VAT number*");
    }

    [Fact]
    public void Money_AdditionAndSubtraction_ShouldSucceedForSameCurrency()
    {
        // Arrange
        var m1 = new Money(100.50m, Currency.EUR);
        var m2 = new Money(49.50m, Currency.EUR);

        // Act
        var sum = m1 + m2;
        var diff = m1 - m2;

        // Assert
        _ = sum.Amount.Should().Be(150.00m);
        _ = sum.Currency.Should().Be(Currency.EUR);
        _ = diff.Amount.Should().Be(51.00m);
    }

    [Fact]
    public void Money_Addition_DifferentCurrencies_ShouldThrowInvalidOperation()
    {
        // Arrange
        var eur = new Money(100m, Currency.EUR);
        var chf = new Money(100m, Currency.CHF);

        // Act
        var act = () => _ = eur + chf;

        // Assert
        _ = act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot operate on different currencies*");
    }

    [Fact]
    public void Money_ToSwissCashRounding_ShouldRoundCorrectly()
    {
        // Test 0.05 rounding:
        // 10.02 -> 10.00
        // 10.03 -> 10.05
        // 10.07 -> 10.05
        // 10.08 -> 10.10
        _ = new Money(10.02m, Currency.CHF).ToSwissCashRounding().Amount.Should().Be(10.00m);
        _ = new Money(10.03m, Currency.CHF).ToSwissCashRounding().Amount.Should().Be(10.05m);
        _ = new Money(10.07m, Currency.CHF).ToSwissCashRounding().Amount.Should().Be(10.05m);
        _ = new Money(10.08m, Currency.CHF).ToSwissCashRounding().Amount.Should().Be(10.10m);
    }

    [Fact]
    public void QrIban_ValidSwissQrIban_ShouldInstantiate()
    {
        // Act
        var qrIban = new QrIban("CH4431999123456789012");

        // Assert
        _ = qrIban.Value.Should().Be("CH4431999123456789012");
    }

    [Fact]
    public void QrIban_StandardIban_ShouldThrowArgumentException()
    {
        // IID 00799 is UBS, not in range 30000..31999
        var act = () => new QrIban("CH9300799123456789012");

        // Assert
        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid Swiss/Liechtenstein QR-IBAN*");
    }
}
