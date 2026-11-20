// <copyright file="SyncRetryPolicy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Exponential back-off schedule for outbox deliveries.
/// </summary>
public static class SyncRetryPolicy
{
    /// <summary>
    /// The attempts after which an entry is marked as failed.
    /// </summary>
    public const int MaxAttempts = 8;

    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromHours(1);

    /// <summary>
    /// Computes the wait before the next attempt.
    /// </summary>
    /// <param name="attempts">The number of attempts already made (at least 1).</param>
    /// <returns>The delay, doubling per attempt and capped at one hour.</returns>
    public static TimeSpan NextDelay(int attempts)
    {
        var exponent = Math.Clamp(attempts - 1, 0, 20);
        var seconds = BaseDelay.TotalSeconds * Math.Pow(2, exponent);
        return TimeSpan.FromSeconds(Math.Min(seconds, MaxDelay.TotalSeconds));
    }

    /// <summary>
    /// Gets a value indicating whether another attempt is allowed.
    /// </summary>
    /// <param name="attempts">The number of attempts already made.</param>
    /// <returns>True while the attempts are below <see cref="MaxAttempts"/>.</returns>
    public static bool CanRetry(int attempts) => attempts < MaxAttempts;
}
