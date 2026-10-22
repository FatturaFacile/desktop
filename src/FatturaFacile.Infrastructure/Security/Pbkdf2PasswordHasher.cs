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

    /// <summary>
    /// Gets the number of PBKDF2 derivation iterations.
    /// </summary>
    private const int Iterations = 100_000;

    /// <summary>
    /// Gets the pseudorandom function hash algorithm.
    /// </summary>
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    /// <inheritdoc/>
    public void CreateHash(
        string     password,
        out string hash,
        out string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: password);

        var saltBytes = RandomNumberGenerator.GetBytes(count: SaltSize);

        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                                                  password: password,
                                                  salt: saltBytes,
                                                  iterations: Iterations,
                                                  hashAlgorithm: Algorithm,
                                                  outputLength: KeySize);

        salt = Convert.ToBase64String(inArray: saltBytes);
        hash = Convert.ToBase64String(inArray: hashBytes);
    }

    /// <inheritdoc/>
    public bool VerifyHash(
        string password,
        string hash,
        string salt)
    {
        if (string.IsNullOrWhiteSpace(value: password) || string.IsNullOrWhiteSpace(value: hash) || string.IsNullOrWhiteSpace(value: salt))
        {
            return false;
        }

        try
        {
            var saltBytes         = Convert.FromBase64String(s: salt);
            var expectedHashBytes = Convert.FromBase64String(s: hash);

            var computedHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                                                              password: password,
                                                              salt: saltBytes,
                                                              iterations: Iterations,
                                                              hashAlgorithm: Algorithm,
                                                              outputLength: expectedHashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(
                                                           left: computedHashBytes,
                                                           right: expectedHashBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

}
