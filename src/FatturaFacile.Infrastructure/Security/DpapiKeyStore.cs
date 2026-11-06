// <copyright file="DpapiKeyStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Stores the field encryption key protected with Windows DPAPI (machine scope, application entropy).
/// Losing this file makes encrypted columns unreadable: include it in the backup of the installation.
/// </summary>
public static class DpapiKeyStore
{
    private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("FatturaFacile.FieldEncryption.v1");

    /// <summary>
    /// Gets the default key file path (%LOCALAPPDATA%\FatturaFacile\field-encryption.key).
    /// </summary>
    public static string DefaultPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FatturaFacile",
        "field-encryption.key");

    /// <summary>
    /// Loads the key, creating and persisting a new random 256-bit key on first use.
    /// </summary>
    /// <param name="path">The key file path.</param>
    /// <returns>The clear key bytes.</returns>
    public static byte[] LoadOrCreate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (File.Exists(path))
        {
            return ProtectedData.Unprotect(File.ReadAllBytes(path), _entropy, DataProtectionScope.LocalMachine);
        }

        var key = RandomNumberGenerator.GetBytes(32);
        Save(path, key);
        return key;
    }

    /// <summary>
    /// Protects the key with DPAPI for this machine and writes it, replacing any existing key file.
    /// </summary>
    /// <param name="path">The key file path.</param>
    /// <param name="key">The clear 256-bit key.</param>
    public static void Save(string path, byte[] key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(key);

        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(path, ProtectedData.Protect(key, _entropy, DataProtectionScope.LocalMachine));
    }
}
