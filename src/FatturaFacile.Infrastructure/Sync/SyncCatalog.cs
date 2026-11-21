// <copyright file="SyncCatalog.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.Frozen;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// Describes which local entities are mirrored with which web-api entity and route.
/// </summary>
/// <remarks>Adding an entity to the sync means adding it here and registering an <see cref="EfSyncEntityHandler{TEntity}"/>.</remarks>
public static class SyncCatalog
{
    /// <summary>
    /// The entity name of contacts in the delta feed.
    /// </summary>
    public const string Contacts = "contacts";

    /// <summary>
    /// The entity name of products in the delta feed.
    /// </summary>
    public const string Products = "products";

    private static readonly FrozenDictionary<Type, string> WritableEntities = new Dictionary<Type, string>
    {
        [typeof(Contact)] = Contacts,
    }.ToFrozenDictionary();

    /// <summary>
    /// Tries to resolve the entity name of a CLR type whose local changes are pushed to the server.
    /// </summary>
    /// <param name="clrType">The entity type.</param>
    /// <param name="entityName">The sync entity name.</param>
    /// <returns>True when local changes of the type are queued in the outbox.</returns>
    public static bool TryGetWritable(Type clrType, out string entityName)
    {
        ArgumentNullException.ThrowIfNull(clrType);
        return WritableEntities.TryGetValue(clrType, out entityName!);
    }
}
