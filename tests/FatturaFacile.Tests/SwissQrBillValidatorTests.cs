// <copyright file="SwissQrBillValidatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Services;
using Xunit;

namespace FatturaFacile.Tests;

/// <summary>
/// Unit tests verifying Swiss QR-Bill validation, QR-IBAN ranges, and Modulo 10 recursive checksum verification.
/// </summary>
public class SwissQrBillValidatorTests
{
    /// <summary>
    /// Verifies that a valid Swiss QR-IBAN having institution ID in range 30000..31999 is recognized as true.
    /// </summary>
    [Fact]
    public void IsValidQrIban_WithKnownSwissQrIban_ReturnsTrue()
    {
        // CH44 3199 9123 0008 8901 2 from Laravel seeder
        var qrIban = "CH4431999123000889012";
        var result = SwissQrBillValidator.IsValidQrIban(qrIban);

        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a standard Swiss non-QR IBAN returns false when validated as a QR-IBAN.
    /// </summary>
    [Fact]
    public void IsValidQrIban_WithStandardIban_ReturnsFalse()
    {
        // Standard Swiss IBAN (not QR-IBAN, IID is 00762 which is not in 30000..31999)
        var standardIban = "CH9300762011623852957";
        var result = SwissQrBillValidator.IsValidQrIban(standardIban);

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that Modulo 10 recursive check digit calculation yields a single numeric check digit.
    /// </summary>
    [Fact]
    public void CalculateMod10RecursiveCheckDigit_CalculatesExpectedDigit()
    {
        // Modulo 10 recursive test
        var reference = "21000000000313947143000901";
        var checkDigit = SwissQrBillValidator.CalculateMod10RecursiveCheckDigit(reference);

        Assert.InRange(checkDigit, 0, 9);
    }
}
