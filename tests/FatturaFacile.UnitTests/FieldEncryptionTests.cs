// <copyright file="FieldEncryptionTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using FatturaFacile.Infrastructure.Security;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests for AES-GCM column encryption.
/// </summary>
public sealed class FieldEncryptionTests
{
    /// <summary>
    /// Verifies round trip, randomized nonces, legacy clear text passthrough and tamper detection.
    /// </summary>
    [Fact]
    public void Encryptor_RoundTripsAndDetectsTampering()
    {
        var encryptor = new AesGcmFieldEncryptor(RandomNumberGenerator.GetBytes(32));

        var first = encryptor.Encrypt("756.1234.5678.97");
        var second = encryptor.Encrypt("756.1234.5678.97");

        Assert.StartsWith(AesGcmFieldEncryptor.Prefix, first, StringComparison.Ordinal);
        Assert.False(string.Equals(first, second, StringComparison.Ordinal));
        Assert.Equal("756.1234.5678.97", encryptor.Decrypt(first));
        Assert.Equal("legacy clear text", encryptor.Decrypt("legacy clear text"));
        Assert.Null(encryptor.Encrypt(plainText: null));

        var tampered = first![..^4] + (first[^4] == 'A' ? "B" : "A") + first[^3..];
        _ = Assert.ThrowsAny<CryptographicException>(() => encryptor.Decrypt(tampered));

        var otherKey = new AesGcmFieldEncryptor(RandomNumberGenerator.GetBytes(32));
        _ = Assert.ThrowsAny<CryptographicException>(() => otherKey.Decrypt(first));
    }
}
