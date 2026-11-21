// <copyright file="AuditDeviceKeyProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Cryptography;
using FatturaFacile.Application.Security;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Keeps the device HMAC key and device identifier in a single file protected by an <see cref="IKeyProtector"/> (DPAPI).
/// Layout of the clear blob: 16 bytes device GUID followed by the 32-byte key.
/// </summary>
public sealed class AuditDeviceKeyProvider : IAuditDeviceKeyProvider
{
    private const int GuidLength = 16;
    private const int KeyLength = 32;

    private readonly IKeyProtector _protector;
    private readonly string _path;
    private readonly Lock _gate = new();
    private byte[]? _key;
    private string? _deviceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditDeviceKeyProvider"/> class.
    /// </summary>
    /// <param name="protector">The key protector.</param>
    /// <param name="path">The key file path.</param>
    public AuditDeviceKeyProvider(IKeyProtector protector, string path)
    {
        ArgumentNullException.ThrowIfNull(protector);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _protector = protector;
        _path = path;
    }

    /// <inheritdoc/>
    public string GetDeviceId()
    {
        Load();
        return _deviceId!;
    }

    /// <inheritdoc/>
    public byte[] GetKey()
    {
        Load();
        return [.. _key!];
    }

    private void Load()
    {
        lock (_gate)
        {
            if (_key is not null)
            {
                return;
            }

            byte[] blob;

            if (File.Exists(_path))
            {
                blob = _protector.Unprotect(File.ReadAllBytes(_path));
                if (blob.Length != GuidLength + KeyLength)
                {
                    throw new CryptographicException("The audit device key file is corrupt.");
                }
            }
            else
            {
                blob = [.. Guid.NewGuid().ToByteArray(), .. RandomNumberGenerator.GetBytes(KeyLength)];
                Save(blob);
            }

            _deviceId = new Guid(blob.AsSpan(0, GuidLength)).ToString("D");
            _key = blob[GuidLength..];
        }
    }

    private void Save(byte[] blob)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        var temporary = _path + ".tmp";
        File.WriteAllBytes(temporary, _protector.Protect(blob));
        File.Move(temporary, _path, overwrite: true);
    }
}
