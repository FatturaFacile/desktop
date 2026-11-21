// <copyright file="IAuditDeviceKeyProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Provides the device key that signs the local audit chain and the stable device identifier.
/// </summary>
public interface IAuditDeviceKeyProvider
{
    /// <summary>
    /// Gets the stable identifier of this device.
    /// </summary>
    /// <returns>The device identifier.</returns>
    public string GetDeviceId();

    /// <summary>
    /// Gets the 256-bit HMAC key of the device (created and protected on first use).
    /// </summary>
    /// <returns>The clear key; callers must not persist it.</returns>
    public byte[] GetKey();
}
