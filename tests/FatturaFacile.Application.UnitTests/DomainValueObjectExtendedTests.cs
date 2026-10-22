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
        var zeroEur = Money.Zero(currency: Currency.EUR);
        var zeroChf = Money.Zero(currency: Currency.CHF);

        // Assert
        _ = zeroEur.Amount.Should().Be(expected: 0m);
        _ = zeroEur.Currency.Should().Be(expected: Currency.EUR);
        _ = zeroChf.Amount.Should().Be(expected: 0m);
        _ = zeroChf.Currency.Should().Be(expected: Currency.CHF);
    }

    /// <summary>
    /// Tests that Money Euro and Chf factory methods set amount and currency.
    /// </summary>
    [Fact]
    public void Money_FactoryMethods_ShouldInstantiateWithCorrectCurrency()
    {
        // Act
        var eur = Money.Euro(amount: 125.75m);
        var chf = Money.Chf(amount: 89.20m);

        // Assert
        _ = eur.Amount.Should().Be(expected: 125.75m);
        _ = eur.Currency.Should().Be(expected: Currency.EUR);
        _ = chf.Amount.Should().Be(expected: 89.20m);
        _ = chf.Currency.Should().Be(expected: Currency.CHF);
    }

    /// <summary>
    /// Tests multiplication of Money with decimal scalar.
    /// </summary>
    [Fact]
    public void Money_MultiplyOperator_ShouldScaleAmountCorrectly()
    {
        // Arrange
        var money = Money.Euro(amount: 50.00m);

        // Act
        var scaled = money * 3m;

        // Assert
        _ = scaled.Amount.Should().Be(expected: 150.00m);
        _ = scaled.Currency.Should().Be(expected: Currency.EUR);
    }

    /// <summary>
    /// Tests subtraction of Money with different currencies throws.
    /// </summary>
    [Fact]
    public void Money_Subtraction_DifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eur = Money.Euro(amount: 100m);
        var chf = Money.Chf(amount: 50m);

        // Act
        var act = () => _ = eur - chf;

        // Assert
        _ = act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage(expectedWildcardPattern: "*Cannot operate on different currencies*");
    }

    /// <summary>
    /// Tests comparison operators on Money with same currency.
    /// </summary>
    [Fact]
    public void Money_ComparisonOperators_ShouldCompareAmountsCorrectly()
    {
        // Arrange
        var lower  = Money.Euro(amount: 50m);
        var higher = Money.Euro(amount: 100m);
        var equal  = Money.Euro(amount: 50m);

        // Assert
        _ = (lower  < higher).Should().BeTrue();
        _ = (lower  <= higher).Should().BeTrue();
        _ = (lower  <= equal).Should().BeTrue();
        _ = (higher > lower).Should().BeTrue();
        _ = (higher >= lower).Should().BeTrue();
        _ = (lower  >= equal).Should().BeTrue();
        _ = (lower  == equal).Should().BeTrue();
        _ = (lower  != higher).Should().BeTrue();
    }

    /// <summary>
    /// Tests comparison with null and non-Money objects.
    /// </summary>
    [Fact]
    public void Money_CompareTo_EdgeCases_ShouldHandleNullAndIncompatibleTypes()
    {
        // Arrange
        var         money      = Money.Euro(amount: 100m);
        IComparable comparable = money;

        // Act & Assert
        _ = money.CompareTo(other: null).Should().Be(expected: 1);
        _ = comparable.CompareTo(obj: null).Should().Be(expected: 1);
        _ = comparable.CompareTo(obj: Money.Euro(amount: 100m)).Should().Be(expected: 0);

        var act = () => comparable.CompareTo(obj: "not money");

        _ = act.Should()
               .Throw<ArgumentException>()
               .WithMessage(expectedWildcardPattern: "*Object must be of type Money*");
    }

    /// <summary>
    /// Tests Swiss cash rounding does not alter non-CHF currencies.
    /// </summary>
    [Fact]
    public void Money_ToSwissCashRounding_NonChf_ShouldReturnUnmodified()
    {
        // Arrange
        var eur = Money.Euro(amount: 10.03m);

        // Act
        var rounded = eur.ToSwissCashRounding();

        // Assert
        _ = rounded.Amount.Should().Be(expected: 10.03m);
        _ = rounded.Currency.Should().Be(expected: Currency.EUR);
    }

    /// <summary>
    /// Tests Money ToString and GetHashCode formatting.
    /// </summary>
    [Fact]
    public void Money_ToStringAndHashCode_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.Euro(amount: 1234.50m);
        var same  = Money.Euro(amount: 1234.50m);

        // Assert
        _ = money.ToString().Should().Contain(expected: "EUR");
        _ = money.GetHashCode().Should().Be(expected: same.GetHashCode());
    }

    /// <summary>
    /// Verifies that constructing an <see cref="Address"/> with invalid or empty fields
    /// throws an <see cref="ArgumentException"/> identifying the offending parameter name.
    /// </summary>
    /// <param name="street">The street address to test.</param>
    /// <param name="city">The city name to test.</param>
    /// <param name="postalCode">The postal code to test.</param>
    /// <param name="paramName">The name of the parameter expected in the thrown exception.</param>
    [Theory]
    [InlineData(
                   data: ["", "Milano", "20100", "street"])]
    [InlineData(
                   data: ["   ", "Milano", "20100", "street"])]
    [InlineData(
                   data: ["Via Roma 1", "", "20100", "city"])]
    [InlineData(
                   data: ["Via Roma 1", "Milano", "", "postalCode"])]
    public void Address_InvalidParameters_ShouldThrowArgumentException(
        string street,
        string city,
        string postalCode,
        string paramName)
    {
        // Act
        var act = () => new Address(
                                    street: street,
                                    city: city,
                                    postalCode: postalCode,
                                    provinceOrCanton: "MI",
                                    countryCode: "IT");

        // Assert
        _ = act.Should()
               .Throw<ArgumentException>()
               .WithParameterName(paramName: paramName);
    }

    /// <summary>
    /// Tests Address default country code fallback and formatting.
    /// </summary>
    [Fact]
    public void Address_DefaultCountryCode_ShouldDefaultToItaly()
    {
        // Act
        var addr = new Address(
                               street: "Via Dante 5",
                               city: "Torino",
                               postalCode: "10121",
                               provinceOrCanton: "TO",
                               countryCode: "");

        // Assert
        _ = addr.CountryCode.Should().Be(expected: "IT");
        _ = addr.ProvinceOrCanton.Should().Be(expected: "TO");
        _ = addr.ToString().Should().Be(expected: "Via Dante 5, 10121 Torino (TO) - IT");
    }

    /// <summary>
    /// Tests FiscalCode validation for 11-digit legal entity codes.
    /// </summary>
    [Fact]
    public void FiscalCode_NumericEntityCode_ShouldBeValid()
    {
        // Arrange & Act
        var fc = new FiscalCode(value: "12345678901");

        // Assert
        _ = fc.Value.Should().Be(expected: "12345678901");
        _ = FiscalCode.IsValid(code: "12345678901").Should().BeTrue();
    }

    /// <summary>
    /// Tests FiscalCode with null or empty values.
    /// </summary>
    [Theory]
    [InlineData(data: "")]
    [InlineData(data: "   ")]
    public void FiscalCode_EmptyOrWhitespace_ShouldThrowArgumentException(string value)
    {
        // Act
        var act = () => new FiscalCode(value: value);

        // Assert
        _ = act.Should()
               .Throw<ArgumentException>()
               .WithMessage(expectedWildcardPattern: "*Fiscal code cannot be empty*");
    }

    /// <summary>
    /// Tests FiscalCode IsValid returns false for null.
    /// </summary>
    [Fact]
    public void FiscalCode_IsValid_NullOrInvalidLength_ShouldReturnFalse()
    {
        _ = FiscalCode.IsValid(code: null!).Should().BeFalse();
        _ = FiscalCode.IsValid(code: "SHORT").Should().BeFalse();
        _ = FiscalCode.IsValid(code: "TOOLONG1234567890123456").Should().BeFalse();
    }

    /// <summary>
    /// Tests VatNumber normalization and validation for Italian Partita IVA with IT prefix.
    /// </summary>
    [Fact]
    public void VatNumber_ItalianVatWithPrefix_ShouldStripPrefixAndValidate()
    {
        // Act
        var vat = new VatNumber(
                                value: "IT 12345678901",
                                countryCode: "IT");

        // Assert
        _ = vat.Value.Should().Be(expected: "12345678901");
        _ = vat.CountryCode.Should().Be(expected: "IT");
        _ = vat.ToString().Should().Be(expected: "IT12345678901");
    }

    /// <summary>
    /// Tests VatNumber for generic other country codes.
    /// </summary>
    [Fact]
    public void VatNumber_OtherCountry_ShouldValidateMinimumLength()
    {
        // Act
        var vat = new VatNumber(
                                value: "DE123456789",
                                countryCode: "DE");

        // Assert
        _ = vat.Value.Should().Be(expected: "DE123456789");
        _ = vat.CountryCode.Should().Be(expected: "DE");
        _ = vat.ToString().Should().Be(expected: "DE123456789");
    }

    /// <summary>
    /// Tests VatNumber with empty value throws ArgumentException.
    /// </summary>
    [Fact]
    public void VatNumber_Empty_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new VatNumber(
                                      value: "",
                                      countryCode: "IT");

        // Assert
        _ = act.Should()
               .Throw<ArgumentException>()
               .WithMessage(expectedWildcardPattern: "*VAT number cannot be empty*");
    }

    /// <summary>
    /// Tests VatNumber IsValid returns false for null or empty.
    /// </summary>
    [Fact]
    public void VatNumber_IsValid_Null_ShouldReturnFalse() => _ = VatNumber.IsValid(
                                                                                    cleaned: null!,
                                                                                    countryCode: "IT")
                                                                           .Should()
                                                                           .BeFalse();

    /// <summary>
    /// Tests QrIban for Liechtenstein valid QR-IBAN.
    /// </summary>
    [Fact]
    public void QrIban_LiechtensteinValidQrIban_ShouldInstantiate()
    {
        // Act
        var qrIban = new QrIban(value: "LI4431500123456789012");

        // Assert
        _ = qrIban.Value.Should().Be(expected: "LI4431500123456789012");
        _ = qrIban.ToFormattedString().Should().Be(expected: "LI44 3150 0123 4567 8901 2");
    }

    /// <summary>
    /// Tests QrIban constructor with null or empty input.
    /// </summary>
    [Theory]
    [InlineData(data: "")]
    [InlineData(data: "   ")]
    public void QrIban_Empty_ShouldThrowArgumentException(string value)
    {
        // Act
        var act = () => new QrIban(value: value);

        // Assert
        _ = act.Should()
               .Throw<ArgumentException>()
               .WithMessage(expectedWildcardPattern: "*QR-IBAN cannot be empty*");
    }

    /// <summary>
    /// Tests QrIban IsValid edge cases.
    /// </summary>
    [Fact]
    public void QrIban_IsValid_EdgeCases_ShouldReturnFalse()
    {
        _ = QrIban.IsValid(iban: null!).Should().BeFalse();
        _ = QrIban.IsValid(iban: "CH123").Should().BeFalse();
        _ = QrIban.IsValid(iban: "DE4431999123456789012").Should().BeFalse();
        _ = QrIban.IsValid(iban: "CH44XXXXX123456789012").Should().BeFalse();
    }

}
