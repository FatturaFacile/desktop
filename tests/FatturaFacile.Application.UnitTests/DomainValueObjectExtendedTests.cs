// <copyright file="DomainValueObjectExtendedTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Unit tests for domain value objects including Money, Address, FiscalCode, VatNumber, and QrIban.
/// </summary>
public sealed class DomainValueObjectExtendedTests
{
    /// <summary>
    /// Tests that Money creates zero instance correctly.
    /// </summary>
    [Fact]
    public void Money_Zero_ShouldCreateZeroAmountWithSpecifiedCurrency()
    {
        // Act
        var zeroEur = Money.Zero(Currency.EUR);
        var zeroChf = Money.Zero(Currency.CHF);

        // Assert
        _ = zeroEur.Amount.Should().Be(0m);
        _ = zeroEur.Currency.Should().Be(Currency.EUR);
        _ = zeroChf.Amount.Should().Be(0m);
        _ = zeroChf.Currency.Should().Be(Currency.CHF);
    }

    /// <summary>
    /// Tests that Money Euro and Chf factory methods set amount and currency.
    /// </summary>
    [Fact]
    public void Money_FactoryMethods_ShouldInstantiateWithCorrectCurrency()
    {
        // Act
        var eur = Money.Euro(125.75m);
        var chf = Money.Chf(89.20m);

        // Assert
        _ = eur.Amount.Should().Be(125.75m);
        _ = eur.Currency.Should().Be(Currency.EUR);
        _ = chf.Amount.Should().Be(89.20m);
        _ = chf.Currency.Should().Be(Currency.CHF);
    }

    /// <summary>
    /// Tests multiplication of Money with decimal scalar.
    /// </summary>
    [Fact]
    public void Money_MultiplyOperator_ShouldScaleAmountCorrectly()
    {
        // Arrange
        var money = Money.Euro(50.00m);

        // Act
        var scaled = money * 3m;

        // Assert
        _ = scaled.Amount.Should().Be(150.00m);
        _ = scaled.Currency.Should().Be(Currency.EUR);
    }

    /// <summary>
    /// Tests subtraction of Money with different currencies throws.
    /// </summary>
    [Fact]
    public void Money_Subtraction_DifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eur = Money.Euro(100m);
        var chf = Money.Chf(50m);

        // Act
        var act = () => _ = eur - chf;

        // Assert
        _ = act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot operate on different currencies*");
    }

    /// <summary>
    /// Tests comparison operators on Money with same currency.
    /// </summary>
    [Fact]
    public void Money_ComparisonOperators_ShouldCompareAmountsCorrectly()
    {
        // Arrange
        var lower = Money.Euro(50m);
        var higher = Money.Euro(100m);
        var equal = Money.Euro(50m);

        // Assert
        _ = (lower < higher).Should().BeTrue();
        _ = (lower <= higher).Should().BeTrue();
        _ = (lower <= equal).Should().BeTrue();
        _ = (higher > lower).Should().BeTrue();
        _ = (higher >= lower).Should().BeTrue();
        _ = (lower >= equal).Should().BeTrue();
        _ = (lower == equal).Should().BeTrue();
        _ = (lower != higher).Should().BeTrue();
    }

    /// <summary>
    /// Tests comparison with null and non-Money objects.
    /// </summary>
    [Fact]
    public void Money_CompareTo_EdgeCases_ShouldHandleNullAndIncompatibleTypes()
    {
        // Arrange
        var money = Money.Euro(100m);
        IComparable comparable = money;

        // Act & Assert
        _ = money.CompareTo(null).Should().Be(1);
        _ = comparable.CompareTo(null).Should().Be(1);
        _ = comparable.CompareTo(Money.Euro(100m)).Should().Be(0);

        var act = () => comparable.CompareTo("not money");
        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("*Object must be of type Money*");
    }

    /// <summary>
    /// Tests Swiss cash rounding does not alter non-CHF currencies.
    /// </summary>
    [Fact]
    public void Money_ToSwissCashRounding_NonChf_ShouldReturnUnmodified()
    {
        // Arrange
        var eur = Money.Euro(10.03m);

        // Act
        var rounded = eur.ToSwissCashRounding();

        // Assert
        _ = rounded.Amount.Should().Be(10.03m);
        _ = rounded.Currency.Should().Be(Currency.EUR);
    }

    /// <summary>
    /// Tests Money ToString and GetHashCode formatting.
    /// </summary>
    [Fact]
    public void Money_ToStringAndHashCode_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.Euro(1234.50m);
        var same = Money.Euro(1234.50m);

        // Assert
        _ = money.ToString().Should().Contain("EUR");
        _ = money.GetHashCode().Should().Be(same.GetHashCode());
    }

    /// <summary>
    /// Tests Address constructor validations for empty fields.
    /// </summary>
    [Theory]
    [InlineData(
        "",
        "Milano",
        "20100",
        "street")]
    [InlineData(
        "   ",
        "Milano",
        "20100",
        "street")]
    [InlineData(
        "Via Roma 1",
        "",
        "20100",
        "city")]
    [InlineData(
        "Via Roma 1",
        "Milano",
        "",
        "postalCode")]
    public void Address_InvalidParameters_ShouldThrowArgumentException(
        string street,
        string city,
        string postalCode,
        string paramName)
    {
        // Act
        var act = () => new Address(
            street,
            city,
            postalCode,
            "MI",
            "IT");

        // Assert
        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName(paramName);
    }

    /// <summary>
    /// Tests Address default country code fallback and formatting.
    /// </summary>
    [Fact]
    public void Address_DefaultCountryCode_ShouldDefaultToItaly()
    {
        // Act
        var addr = new Address(
            "Via Dante 5",
            "Torino",
            "10121",
            "TO",
            "");

        // Assert
        _ = addr.CountryCode.Should().Be("IT");
        _ = addr.ProvinceOrCanton.Should().Be("TO");
        _ = addr.ToString().Should().Be("Via Dante 5, 10121 Torino (TO) - IT");
    }

    /// <summary>
    /// Tests FiscalCode validation for 11-digit legal entity codes.
    /// </summary>
    [Fact]
    public void FiscalCode_NumericEntityCode_ShouldBeValid()
    {
        // Arrange & Act
        var fc = new FiscalCode("12345678901");

        // Assert
        _ = fc.Value.Should().Be("12345678901");
        _ = FiscalCode.IsValid("12345678901").Should().BeTrue();
    }

    /// <summary>
    /// Tests FiscalCode with null or empty values.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FiscalCode_EmptyOrWhitespace_ShouldThrowArgumentException(
        string value)
    {
        // Act
        var act = () => new FiscalCode(value);

        // Assert
        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("*Fiscal code cannot be empty*");
    }

    /// <summary>
    /// Tests FiscalCode IsValid returns false for null.
    /// </summary>
    [Fact]
    public void FiscalCode_IsValid_NullOrInvalidLength_ShouldReturnFalse()
    {
        _ = FiscalCode.IsValid(null!).Should().BeFalse();
        _ = FiscalCode.IsValid("SHORT").Should().BeFalse();
        _ = FiscalCode.IsValid("TOOLONG1234567890123456").Should().BeFalse();
    }

    /// <summary>
    /// Tests VatNumber normalization and validation for Italian Partita IVA with IT prefix.
    /// </summary>
    [Fact]
    public void VatNumber_ItalianVatWithPrefix_ShouldStripPrefixAndValidate()
    {
        // Act
        var vat = new VatNumber(
            "IT 12345678901",
            "IT");

        // Assert
        _ = vat.Value.Should().Be("12345678901");
        _ = vat.CountryCode.Should().Be("IT");
        _ = vat.ToString().Should().Be("IT12345678901");
    }

    /// <summary>
    /// Tests VatNumber for generic other country codes.
    /// </summary>
    [Fact]
    public void VatNumber_OtherCountry_ShouldValidateMinimumLength()
    {
        // Act
        var vat = new VatNumber(
            "DE123456789",
            "DE");

        // Assert
        _ = vat.Value.Should().Be("DE123456789");
        _ = vat.CountryCode.Should().Be("DE");
        _ = vat.ToString().Should().Be("DE123456789");
    }

    /// <summary>
    /// Tests VatNumber with empty value throws ArgumentException.
    /// </summary>
    [Fact]
    public void VatNumber_Empty_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new VatNumber(
            "",
            "IT");

        // Assert
        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("*VAT number cannot be empty*");
    }

    /// <summary>
    /// Tests VatNumber IsValid returns false for null or empty.
    /// </summary>
    [Fact]
    public void VatNumber_IsValid_Null_ShouldReturnFalse() => _ = VatNumber.IsValid(
        null!,
        "IT").Should().BeFalse();

    /// <summary>
    /// Tests QrIban for Liechtenstein valid QR-IBAN.
    /// </summary>
    [Fact]
    public void QrIban_LiechtensteinValidQrIban_ShouldInstantiate()
    {
        // Act
        var qrIban = new QrIban("LI4431500123456789012");

        // Assert
        _ = qrIban.Value.Should().Be("LI4431500123456789012");
        _ = qrIban.ToFormattedString().Should().Be("LI44 3150 0123 4567 8901 2");
    }

    /// <summary>
    /// Tests QrIban constructor with null or empty input.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void QrIban_Empty_ShouldThrowArgumentException(
        string value)
    {
        // Act
        var act = () => new QrIban(value);

        // Assert
        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("*QR-IBAN cannot be empty*");
    }

    /// <summary>
    /// Tests QrIban IsValid edge cases.
    /// </summary>
    [Fact]
    public void QrIban_IsValid_EdgeCases_ShouldReturnFalse()
    {
        _ = QrIban.IsValid(null!).Should().BeFalse();
        _ = QrIban.IsValid("CH123").Should().BeFalse();
        _ = QrIban.IsValid("DE4431999123456789012").Should().BeFalse();
        _ = QrIban.IsValid("CH44XXXXX123456789012").Should().BeFalse();
    }
}
