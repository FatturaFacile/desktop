// <copyright file="DpapiKeyProtector.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using FatturaFacile.Application.Security;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Protects keys with the Windows Data Protection API in the scope of the current user (with application entropy).
/// </summary>
public sealed class DpapiKeyProtector : IKeyProtector
{
    private static readonly byte[] _entropy = "FatturaFacile.AuditDeviceKey.v1"u8.ToArray();

    /// <inheritdoc/>
    public string Scheme => "DPAPI-CurrentUser";

    /// <inheritdoc/>
    public bool IsDpapi => true;

    /// <inheritdoc/>
    public byte[] Protect(byte[] clear)
    {
        ArgumentNullException.ThrowIfNull(clear);
        return ProtectedData.Protect(clear, _entropy, DataProtectionScope.CurrentUser);
    }

    /// <inheritdoc/>
    public byte[] Unprotect(byte[] protectedData)
    {
        ArgumentNullException.ThrowIfNull(protectedData);
        return ProtectedData.Unprotect(protectedData, _entropy, DataProtectionScope.CurrentUser);
    }
}
