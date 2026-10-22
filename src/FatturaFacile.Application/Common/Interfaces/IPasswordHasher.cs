// <copyright file="IPasswordHasher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Provides cryptographic hashing and verification routines for operator credentials and access secrets.
/// </summary>
public interface IPasswordHasher
{

    /// <summary>
    /// Computes a cryptographically salted password hash.
    /// </summary>
    /// <param name="password">The raw, unhashed plain text password.</param>
    /// <param name="hash">Outputs the resulting computed cryptographic hash string.</param>
    /// <param name="salt">Outputs the unique cryptographic salt used during hashing.</param>
    public void CreateHash(
        string     password,
        out string hash,
        out string salt);

    /// <summary>
    /// Verifies whether the provided raw password matches the persisted cryptographic hash given the corresponding salt.
    /// </summary>
    /// <param name="password">The plain text candidate password to verify.</param>
    /// <param name="hash">The stored expected cryptographic hash.</param>
    /// <param name="salt">The stored salt originally used to compute the hash.</param>
    /// <returns><see langword="true"/> if the password produces a matching hash; otherwise, <see langword="false"/>.</returns>
    public bool VerifyHash(
        string password,
        string hash,
        string salt);

}
