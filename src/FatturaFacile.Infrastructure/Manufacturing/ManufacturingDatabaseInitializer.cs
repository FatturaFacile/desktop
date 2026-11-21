// <copyright file="ManufacturingDatabaseInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <summary>
/// Creates or migrates the manufacturing SQLite database at start-up.
/// </summary>
/// <param name="factory">The context factory.</param>
public sealed class ManufacturingDatabaseInitializer(IDbContextFactory<ManufacturingDbContext> factory)
{
    /// <summary>
    /// Applies the pending migrations (creating the database file when missing) and enables WAL.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the initialisation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        _ = await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken).ConfigureAwait(false);
    }
}
