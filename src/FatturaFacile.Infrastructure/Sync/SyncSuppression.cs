// <copyright file="SyncSuppression.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// Suspends outbox capture while data pulled from the server is written locally, so it is not echoed back.
/// </summary>
public static class SyncSuppression
{
    private static readonly AsyncLocal<bool> Suppressed = new();

    /// <summary>
    /// Gets a value indicating whether outbox capture is suspended on the current async flow.
    /// </summary>
    public static bool IsSuppressed => Suppressed.Value;

    /// <summary>
    /// Suspends capture until the returned scope is disposed.
    /// </summary>
    /// <returns>The scope.</returns>
    public static IDisposable Begin()
    {
        var previous = Suppressed.Value;
        Suppressed.Value = true;
        return new Scope(previous);
    }

    private sealed class Scope(bool previous) : IDisposable
    {
        public void Dispose() => Suppressed.Value = previous;
    }
}
