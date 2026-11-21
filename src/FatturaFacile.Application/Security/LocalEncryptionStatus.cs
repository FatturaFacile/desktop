// <copyright file="LocalEncryptionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// State of the encryption of the local data at rest.
/// </summary>
/// <param name="DatabaseFileEncrypted">Whether the SQLite file itself is encrypted (no plain SQLite header).</param>
/// <param name="FieldEncryptionActive">Whether sensitive columns are encrypted with a key that could be loaded.</param>
/// <param name="KeyProtectedByDpapi">Whether the encryption keys are protected by Windows DPAPI.</param>
/// <param name="ProtectionScheme">The key protection scheme of the device key (for example "DPAPI-CurrentUser").</param>
/// <param name="DatabasePath">The path of the local database.</param>
public sealed record LocalEncryptionStatus(
    bool DatabaseFileEncrypted,
    bool FieldEncryptionActive,
    bool KeyProtectedByDpapi,
    string ProtectionScheme,
    string DatabasePath)
{
    /// <summary>
    /// Gets a value indicating whether local data is protected at rest (file or columns encrypted, and keys protected).
    /// </summary>
    public bool IsProtected => (DatabaseFileEncrypted || FieldEncryptionActive) && KeyProtectedByDpapi;
}
