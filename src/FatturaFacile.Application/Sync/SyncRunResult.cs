// <copyright file="SyncRunResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Summary of one sync pass.
/// </summary>
/// <param name="Pushed">Outbox entries accepted by the server.</param>
/// <param name="Conflicts">Outbox entries that hit a server conflict.</param>
/// <param name="Failed">Outbox entries permanently failed.</param>
/// <param name="Pulled">Records applied locally from the server.</param>
/// <param name="SkippedPulls">Pulled records skipped because of pending local changes.</param>
/// <param name="AuthRequired">Whether the user must sign in again.</param>
/// <param name="Errors">Non-fatal error messages.</param>
public sealed record SyncRunResult(
    int Pushed,
    int Conflicts,
    int Failed,
    int Pulled,
    int SkippedPulls,
    bool AuthRequired,
    IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Gets a result for a pass that did not run (not signed in).
    /// </summary>
    public static SyncRunResult NotAuthenticated { get; } = new(Pushed: 0, Conflicts: 0, Failed: 0, Pulled: 0, SkippedPulls: 0, AuthRequired: true, Errors: []);

    /// <summary>
    /// Gets a result for a pass that did not run (synchronization not configured).
    /// </summary>
    public static SyncRunResult Disabled { get; } = new(Pushed: 0, Conflicts: 0, Failed: 0, Pulled: 0, SkippedPulls: 0, AuthRequired: false, Errors: []);
}
