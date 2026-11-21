// <copyright file="LocalEncryptionStatusServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Cryptography;
using FatturaFacile.Application.Security;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the local encryption status service and of the security settings store.
/// </summary>
public sealed class LocalEncryptionStatusServiceTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "ff-status-" + Guid.NewGuid().ToString("N"));

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalEncryptionStatusServiceTests"/> class.
    /// </summary>
    public LocalEncryptionStatusServiceTests()
    {
        _ = Directory.CreateDirectory(directory);
    }

    /// <summary>
    /// A plain SQLite file with a DPAPI-protected field key reports field encryption and DPAPI, but no file encryption.
    /// </summary>
    [Fact]
    public void PlainSqliteWithProtectedKeys_ReportsFieldEncryptionAndDpapi()
    {
        var paths = Paths();
        File.WriteAllBytes(paths.DatabasePath, [.. "SQLite format 3\0"u8, .. new byte[64]]);
        DpapiKeyStore.Save(paths.KeyPath, RandomNumberGenerator.GetBytes(32));

        var status = Service(paths, new DpapiKeyProtector()).GetStatus();

        Assert.False(status.DatabaseFileEncrypted);
        Assert.True(status.FieldEncryptionActive);
        Assert.True(status.KeyProtectedByDpapi);
        Assert.True(status.IsProtected);
        Assert.Equal("DPAPI-CurrentUser", status.ProtectionScheme);
        Assert.Equal(paths.DatabasePath, status.DatabasePath);
    }

    /// <summary>
    /// A file without the plain SQLite header is reported as encrypted.
    /// </summary>
    [Fact]
    public void FileWithoutSqliteHeader_IsReportedEncrypted()
    {
        var paths = Paths();
        File.WriteAllBytes(paths.DatabasePath, RandomNumberGenerator.GetBytes(4096));

        Assert.True(Service(paths, new FakeKeyProtector()).GetStatus().DatabaseFileEncrypted);
    }

    /// <summary>
    /// Missing files are reported as not protected instead of failing.
    /// </summary>
    [Fact]
    public void NoFiles_IsNotFieldProtected()
    {
        var status = Service(Paths(), new FakeKeyProtector()).GetStatus();

        Assert.False(status.DatabaseFileEncrypted);
        Assert.False(status.FieldEncryptionActive);
    }

    /// <summary>
    /// A key store that is not DPAPI is never reported as DPAPI protected.
    /// </summary>
    [Fact]
    public void NonDpapiProtector_IsNotReportedAsDpapi()
    {
        var paths = Paths();
        DpapiKeyStore.Save(paths.KeyPath, RandomNumberGenerator.GetBytes(32));

        var status = Service(paths, new FakeKeyProtector(isDpapi: false)).GetStatus();

        Assert.False(status.KeyProtectedByDpapi);
        Assert.False(status.IsProtected);
    }

    /// <summary>
    /// A device key file that cannot be unprotected is reported as not protected.
    /// </summary>
    [Fact]
    public void UnreadableDeviceKey_IsNotProtected()
    {
        var paths = Paths();
        var keyFile = Path.Combine(directory, "device.key");
        File.WriteAllBytes(keyFile, [1, 2, 3]);

        var status = new LocalEncryptionStatusService(paths, new FakeKeyProtector(), new AuditDeviceKeyProvider(new FakeKeyProtector(), keyFile)).GetStatus();

        Assert.False(status.KeyProtectedByDpapi);
    }

    /// <summary>
    /// Settings roundtrip and the timeout is clamped to the allowed range.
    /// </summary>
    [Fact]
    public void SettingsStore_RoundtripsAndClamps()
    {
        var store = new JsonSecuritySettingsStore(Path.Combine(directory, "settings.json"));

        Assert.Equal(SecuritySettings.DefaultLockTimeoutMinutes, store.Load().LockTimeoutMinutes);

        store.Save(new SecuritySettings { LockTimeoutMinutes = 5000, IdleLockEnabled = false, RequirePinToUnlock = false });
        var loaded = store.Load();

        Assert.Equal(SecuritySettings.MaxLockTimeoutMinutes, loaded.LockTimeoutMinutes);
        Assert.False(loaded.IdleLockEnabled);
        Assert.False(loaded.RequirePinToUnlock);
    }

    /// <summary>
    /// A corrupted settings file falls back to the defaults.
    /// </summary>
    [Fact]
    public void SettingsStore_CorruptFile_UsesDefaults()
    {
        var path = Path.Combine(directory, "bad.json");
        File.WriteAllText(path, "{ not json");

        Assert.Equal(SecuritySettings.DefaultLockTimeoutMinutes, new JsonSecuritySettingsStore(path).Load().LockTimeoutMinutes);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private DatabasePaths Paths() => new(Path.Combine(directory, "db.sqlite"), Path.Combine(directory, "field.key"));

    private LocalEncryptionStatusService Service(DatabasePaths paths, IKeyProtector protector)
        => new(paths, protector, new AuditDeviceKeyProvider(protector, Path.Combine(directory, "device.key")));
}
