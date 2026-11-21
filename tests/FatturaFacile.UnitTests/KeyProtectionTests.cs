// <copyright file="KeyProtectionTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Cryptography;
using FatturaFacile.Infrastructure.Security;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the DPAPI key protection and of the device key file.
/// </summary>
public sealed class KeyProtectionTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "ff-keys-" + Guid.NewGuid().ToString("N"));

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyProtectionTests"/> class.
    /// </summary>
    public KeyProtectionTests()
    {
        _ = Directory.CreateDirectory(directory);
    }

    /// <summary>
    /// The real DPAPI (current user) protects and recovers a key and the blob is not the clear key.
    /// </summary>
    [Fact]
    public void Dpapi_Roundtrip_RecoversKey()
    {
        var protector = new DpapiKeyProtector();
        var key = RandomNumberGenerator.GetBytes(32);

        var blob = protector.Protect(key);

        Assert.True(protector.IsDpapi);
        Assert.Equal("DPAPI-CurrentUser", protector.Scheme);
        Assert.NotEqual(key, blob);
        Assert.Equal(key, protector.Unprotect(blob));
    }

    /// <summary>
    /// A corrupted DPAPI blob is rejected instead of yielding garbage.
    /// </summary>
    [Fact]
    public void Dpapi_CorruptedBlob_Throws()
    {
        var protector = new DpapiKeyProtector();
        var blob = protector.Protect(RandomNumberGenerator.GetBytes(32));
        blob[^1] ^= 0xFF;

        _ = Assert.ThrowsAny<CryptographicException>(() => protector.Unprotect(blob));
    }

    /// <summary>
    /// The device key is created once, stored only in protected form and reloaded identically.
    /// </summary>
    [Fact]
    public void DeviceKey_IsPersistedProtectedAndStable()
    {
        var path = Path.Combine(directory, "audit.key");
        var first = new AuditDeviceKeyProvider(new FakeKeyProtector(), path);

        var key = first.GetKey();
        var deviceId = first.GetDeviceId();
        var second = new AuditDeviceKeyProvider(new FakeKeyProtector(), path);

        Assert.Equal(32, key.Length);
        Assert.True(Guid.TryParse(deviceId, out _));
        Assert.Equal(key, second.GetKey());
        Assert.Equal(deviceId, second.GetDeviceId());
        Assert.False(File.ReadAllBytes(path).AsSpan().IndexOf(key) >= 0, "The clear key must not be readable in the key file.");
    }

    /// <summary>
    /// With real DPAPI the key file survives a restart and stays unreadable without the protector.
    /// </summary>
    [Fact]
    public void DeviceKey_WithDpapi_Roundtrips()
    {
        var path = Path.Combine(directory, "dpapi.key");
        var key = new AuditDeviceKeyProvider(new DpapiKeyProtector(), path).GetKey();

        Assert.Equal(key, new AuditDeviceKeyProvider(new DpapiKeyProtector(), path).GetKey());
        Assert.False(File.ReadAllBytes(path).AsSpan().IndexOf(key) >= 0);
    }

    /// <summary>
    /// A truncated key file is reported as corrupt.
    /// </summary>
    [Fact]
    public void DeviceKey_CorruptFile_Throws()
    {
        var path = Path.Combine(directory, "bad.key");
        File.WriteAllBytes(path, new FakeKeyProtector().Protect([1, 2, 3]));

        _ = Assert.Throws<CryptographicException>(() => new AuditDeviceKeyProvider(new FakeKeyProtector(), path).GetKey());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
