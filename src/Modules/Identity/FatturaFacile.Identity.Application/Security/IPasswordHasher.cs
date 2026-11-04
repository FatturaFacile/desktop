// <copyright file="IPasswordHasher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Password hashing contract supporting Argon2id.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the raw password string using Argon2id with cryptographically secure salt.
    /// </summary>
    /// <param name="password">The plaintext password.</param>
    /// <returns>The encoded hash string containing algorithm parameters, salt, and key.</returns>
    public string HashPassword(string password);

    /// <summary>
    /// Verifies whether the provided plaintext password matches the stored Argon2id hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="passwordHash">The stored Argon2id hash.</param>
    /// <returns>True if password matches; otherwise false.</returns>
    public bool VerifyPassword(string password, string passwordHash);
}
