// <copyright file="AesGcmFieldEncryptor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Authenticated encryption (AES-256-GCM) of sensitive text columns. Values are stored as
/// "enc:v1:" + Base64(nonce | tag | ciphertext); legacy clear-text values are returned unchanged on read.
/// </summary>
public sealed class AesGcmFieldEncryptor
{
    /// <summary>
    /// The prefix of encrypted values.
    /// </summary>
    public const string Prefix = "enc:v1:";

    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="AesGcmFieldEncryptor"/> class.
    /// </summary>
    /// <param name="key">The 256-bit key.</param>
    /// <exception cref="ArgumentException">Thrown when the key is not 32 bytes long.</exception>
    public AesGcmFieldEncryptor(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length != KeySize)
        {
            throw new ArgumentException("The field encryption key must be 32 bytes long.", nameof(key));
        }

        _key = [.. key];
    }

    /// <summary>
    /// Encrypts a value.
    /// </summary>
    /// <param name="plainText">The clear text (null and empty are stored as is).</param>
    /// <returns>The encrypted representation.</returns>
    public string? Encrypt(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText) || plainText.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return plainText;
        }

        var plain = Encoding.UTF8.GetBytes(plainText);
        var payload = new byte[NonceSize + TagSize + plain.Length];
        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(NonceSize, TagSize);
        var cipher = payload.AsSpan(NonceSize + TagSize);

        RandomNumberGenerator.Fill(nonce);

        using AesGcm aes = new(_key, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag);

        return Prefix + Convert.ToBase64String(payload);
    }

    /// <summary>
    /// Decrypts a value; values without the prefix (legacy clear text) are returned unchanged.
    /// </summary>
    /// <param name="storedValue">The stored value.</param>
    /// <returns>The clear text.</returns>
    /// <exception cref="CryptographicException">Thrown when the value was tampered with or the key is wrong.</exception>
    public string? Decrypt(string? storedValue)
    {
        if (string.IsNullOrEmpty(storedValue) || !storedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return storedValue;
        }

        var payload = Convert.FromBase64String(storedValue[Prefix.Length..]);

        if (payload.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Encrypted value is truncated.");
        }

        var plain = new byte[payload.Length - NonceSize - TagSize];

        using AesGcm aes = new(_key, TagSize);
        aes.Decrypt(
            payload.AsSpan(0, NonceSize),
            payload.AsSpan(NonceSize + TagSize),
            payload.AsSpan(NonceSize, TagSize),
            plain);

        return Encoding.UTF8.GetString(plain);
    }
}
