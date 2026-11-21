// <copyright file="IKeyProtector.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Protects small secrets (keys) at rest. The Windows implementation uses DPAPI; tests use a fake.
/// </summary>
public interface IKeyProtector
{
    /// <summary>
    /// Gets the name of the protection scheme (for example "DPAPI-CurrentUser").
    /// </summary>
    public string Scheme { get; }

    /// <summary>
    /// Gets a value indicating whether the scheme is the Windows Data Protection API.
    /// </summary>
    public bool IsDpapi { get; }

    /// <summary>
    /// Protects clear data.
    /// </summary>
    /// <param name="clear">The clear bytes.</param>
    /// <returns>The protected blob.</returns>
    public byte[] Protect(byte[] clear);

    /// <summary>
    /// Recovers clear data from a protected blob.
    /// </summary>
    /// <param name="protectedData">The protected blob.</param>
    /// <returns>The clear bytes.</returns>
    /// <exception cref="System.Security.Cryptography.CryptographicException">The blob is corrupt or belongs to another user.</exception>
    public byte[] Unprotect(byte[] protectedData);
}
