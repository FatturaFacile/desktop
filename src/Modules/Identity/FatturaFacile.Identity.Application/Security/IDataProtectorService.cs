// <copyright file="IDataProtectorService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Data encryption and blind-indexing helper for Swiss nLPD / FADP compliance.
/// </summary>
public interface IDataProtectorService
{
    /// <summary>
    /// Computes a deterministic SHA-256 blind index hash for indexed email search.
    /// </summary>
    /// <param name="input">The raw sensitive string (e.g., email address).</param>
    /// <returns>A uppercase hex-encoded SHA-256 blind index string.</returns>
    public string ComputeBlindIndex(string input);

    /// <summary>
    /// Encrypts sensitive personal data using AES-256-GCM.
    /// </summary>
    /// <param name="plainText">The plaintext data to encrypt.</param>
    /// <returns>A Base64-encoded combined payload of nonce, tag, and cipher text.</returns>
    public string Encrypt(string plainText);

    /// <summary>
    /// Decrypts AES-256-GCM cipher text.
    /// </summary>
    /// <param name="cipherText">The Base64-encoded encrypted payload.</param>
    /// <returns>The decrypted plaintext string.</returns>
    public string Decrypt(string cipherText);
}
