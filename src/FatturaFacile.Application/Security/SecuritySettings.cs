// <copyright file="SecuritySettings.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// User-adjustable security settings of this device.
/// </summary>
public sealed record SecuritySettings
{
    /// <summary>
    /// The smallest allowed idle lock timeout in minutes.
    /// </summary>
    public const int MinLockTimeoutMinutes = 1;

    /// <summary>
    /// The largest allowed idle lock timeout in minutes.
    /// </summary>
    public const int MaxLockTimeoutMinutes = 240;

    /// <summary>
    /// The default idle lock timeout in minutes.
    /// </summary>
    public const int DefaultLockTimeoutMinutes = 10;

    /// <summary>
    /// Gets the minutes of inactivity after which the session locks (clamped to the allowed range).
    /// </summary>
    public int LockTimeoutMinutes { get; init; } = DefaultLockTimeoutMinutes;

    /// <summary>
    /// Gets a value indicating whether the idle lock is enabled.
    /// </summary>
    public bool IdleLockEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether unlocking requires the PIN/password of the user.
    /// </summary>
    public bool RequirePinToUnlock { get; init; } = true;

    /// <summary>
    /// Returns a copy with the timeout forced into the allowed range.
    /// </summary>
    /// <returns>The normalized settings.</returns>
    public SecuritySettings Normalize() => this with
    {
        LockTimeoutMinutes = Math.Clamp(LockTimeoutMinutes, MinLockTimeoutMinutes, MaxLockTimeoutMinutes),
    };
}
