// <copyright file="DataProtectorService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Provides AES-256-GCM authenticated encryption and blind indexing for PII data protection
/// complying with Swiss nLPD (FADP) and EU GDPR requirements.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DataProtectorService"/> class.
/// </remarks>
/// <param name="key">Optional explicit 256-bit encryption key; otherwise uses default derived key.</param>
public sealed class DataProtectorService(byte[]? key = null) : IDataProtectorService
{
    private const int AesGcmNonceSize = 12;
    private const int AesGcmTagSize = 16;
    private const int MinimumCipherBytesLength = AesGcmNonceSize + AesGcmTagSize;

    /// <summary>
    /// 256-bit key derived or configured.
    /// </summary>
    private readonly byte[] _key = key ?? SHA256.HashData(Encoding.UTF8.GetBytes("FatturaFacile-nLPD-Protection-Key-2026-SwissStandard"));

    /// <inheritdoc />
    public string ComputeBlindIndex(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var normalized = input.Trim().ToUpperInvariant();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    /// <inheritdoc />
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        var nonce = RandomNumberGenerator.GetBytes(AesGcmNonceSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[AesGcmTagSize];

        using var aesGcm = new AesGcm(_key, AesGcmTagSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Combine: nonce (12) + tag (16) + cipherText
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <inheritdoc />
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return string.Empty;
        }

        var fullBytes = Convert.FromBase64String(cipherText);
        if (fullBytes.Length < MinimumCipherBytesLength)
        {
            throw new CryptographicException("Invalid cipher text length.");
        }

        var nonce = new byte[AesGcmNonceSize];
        var tag = new byte[AesGcmTagSize];
        var cipherBytes = new byte[fullBytes.Length - MinimumCipherBytesLength];

        Buffer.BlockCopy(fullBytes, 0, nonce, 0, AesGcmNonceSize);
        Buffer.BlockCopy(fullBytes, AesGcmNonceSize, tag, 0, AesGcmTagSize);
        Buffer.BlockCopy(fullBytes, MinimumCipherBytesLength, cipherBytes, 0, cipherBytes.Length);

        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(_key, AesGcmTagSize);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
