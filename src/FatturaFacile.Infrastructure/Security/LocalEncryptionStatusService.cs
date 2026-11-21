// <copyright file="LocalEncryptionStatusService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Cryptography;
using FatturaFacile.Application.Security;
using FatturaFacile.Infrastructure.Persistence;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Inspects the local SQLite file and the key files to report the encryption state at rest.
/// </summary>
/// <param name="paths">The database and key file locations.</param>
/// <param name="protector">The device key protector.</param>
/// <param name="deviceKeys">The audit device key provider.</param>
public sealed class LocalEncryptionStatusService(DatabasePaths paths, IKeyProtector protector, IAuditDeviceKeyProvider deviceKeys) : ILocalEncryptionStatusService
{
    private static readonly byte[] _sqliteHeader = "SQLite format 3\0"u8.ToArray();

    /// <inheritdoc/>
    public LocalEncryptionStatus GetStatus()
    {
        var fieldKeyProtected = FieldKeyIsLoadable();
        var deviceKeyProtected = DeviceKeyIsLoadable();

        return new LocalEncryptionStatus(
            DatabaseFileEncrypted: DatabaseFileIsEncrypted(),
            FieldEncryptionActive: fieldKeyProtected,
            KeyProtectedByDpapi: protector.IsDpapi && deviceKeyProtected,
            ProtectionScheme: protector.Scheme,
            DatabasePath: paths.DatabasePath);
    }

    private bool DatabaseFileIsEncrypted()
    {
        if (!File.Exists(paths.DatabasePath))
        {
            return false;
        }

        try
        {
            using var stream = new FileStream(paths.DatabasePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var header = new byte[_sqliteHeader.Length];
            var read = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);

            // An empty file is a fresh database, not an encrypted one; anything without the plain SQLite magic is ciphertext.
            return read == header.Length && !header.AsSpan().SequenceEqual(_sqliteHeader);
        }
        catch (IOException)
        {
            return false;
        }
    }

    private bool FieldKeyIsLoadable()
    {
        try
        {
            return File.Exists(paths.KeyPath) && DpapiKeyStore.LoadOrCreate(paths.KeyPath).Length == 32;
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private bool DeviceKeyIsLoadable()
    {
        try
        {
            return deviceKeys.GetKey().Length == 32;
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
