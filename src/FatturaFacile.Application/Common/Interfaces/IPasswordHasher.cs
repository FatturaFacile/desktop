// <copyright file="IPasswordHasher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Password hashing with a slow, salted key-derivation function.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password.
    /// </summary>
    /// <param name="password">The clear-text password.</param>
    /// <returns>The encoded hash including algorithm, iterations and salt.</returns>
    public string Hash(string password);

    /// <summary>
    /// Verifies a password against an encoded hash in constant time.
    /// </summary>
    /// <param name="password">The clear-text password.</param>
    /// <param name="encodedHash">The encoded hash.</param>
    /// <returns><see langword="true"/> when the password matches.</returns>
    public bool Verify(string password, string encodedHash);
}
