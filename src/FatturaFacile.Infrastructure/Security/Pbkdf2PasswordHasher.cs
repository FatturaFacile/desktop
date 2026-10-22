// <copyright file="Pbkdf2PasswordHasher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;

using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Provides PBKDF2 password hashing with SHA-512 and cryptographic salt generation.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{

    /// <summary>
    /// Gets the salt size in bytes (256-bit).
    /// </summary>
    private const int SaltSize = 32;

    /// <summary>
    /// Gets the hash key size in bytes (512-bit).
    /// </summary>
    private const int KeySize = 64;

    private const int Iterations = 100_000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    /// <inheritdoc/>
    public void CreateHash(
        string     password,
        out string hash,
        out string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                                                  password,
                                                  saltBytes,
                                                  Iterations,
                                                  Algorithm,
                                                  KeySize);

        salt = Convert.ToBase64String(saltBytes);
        hash = Convert.ToBase64String(hashBytes);
    }

    /// <inheritdoc/>
    public bool VerifyHash(
        string password,
        string hash,
        string salt)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(salt))
        {
            return false;
        }

        try
        {
            var saltBytes         = Convert.FromBase64String(salt);
            var expectedHashBytes = Convert.FromBase64String(hash);

            var computedHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                                                              password,
                                                              saltBytes,
                                                              Iterations,
                                                              Algorithm,
                                                              expectedHashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(
                                                           computedHashBytes,
                                                           expectedHashBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

}
