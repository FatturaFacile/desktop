// <copyright file="FieldServiceCacheReader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Reads a payload of the local Field Service cache and parses it, with the instant it was pulled.
/// </summary>
public static class FieldServiceCacheReader
{
    /// <summary>
    /// Reads and parses a cached payload.
    /// </summary>
    /// <typeparam name="T">The read model type.</typeparam>
    /// <param name="store">The local cache.</param>
    /// <param name="kind">The kind (see <see cref="FieldServiceCacheKinds"/>).</param>
    /// <param name="key">The key inside the kind.</param>
    /// <param name="parse">The parser of the payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The read model, or null when nothing was pulled yet.</returns>
    public static async Task<FieldServiceCached<T>?> ReadAsync<T>(IFieldServiceStore store, string kind, string key, Func<string, T> parse, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(parse);

        var stored = await store.FindAsync(kind, key, cancellationToken).ConfigureAwait(false);

        return stored is null ? null : new FieldServiceCached<T>(parse(stored.Json), stored.SyncedAtUtc);
    }
}
