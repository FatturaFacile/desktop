// <copyright file="DatabaseMigrator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Applies EF Core migrations. A legacy database created with EnsureCreated (no migration history) is moved
/// to a timestamped backup file and a new, migrated database is created.
/// </summary>
public static class DatabaseMigrator
{
    /// <summary>
    /// Migrates the database to the latest schema.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The path of the legacy backup file, or null when none was needed.</returns>
    public static Task<string?> MigrateAsync(FatturaFacileDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        return MigrateInternalAsync(context, cancellationToken);
    }

    private static async Task<string?> MigrateInternalAsync(FatturaFacileDbContext context, CancellationToken cancellationToken)
    {
        if (!context.Database.IsSqlite())
        {
            _ = await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }

        string? backupPath = null;
        var applied = await context.Database.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false);

        if (!applied.Any() && await LegacySchemaExistsAsync(context, cancellationToken).ConfigureAwait(false))
        {
            backupPath = BackupLegacyDatabase(context);
        }

        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        return backupPath;
    }

    private static async Task<bool> LegacySchemaExistsAsync(FatturaFacileDbContext context, CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'tenants'";
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return Convert.ToInt64(result, CultureInfo.InvariantCulture) > 0;
        }
        finally
        {
            await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        }
    }

    [SuppressMessage("Security", "SCS0018:Path traversal", Justification = "The path comes from the application connection string, not from user input.")]
    private static string BackupLegacyDatabase(FatturaFacileDbContext context)
    {
        using SqliteConnection connection = new(context.Database.GetConnectionString());
        var path = Path.GetFullPath(connection.DataSource);
        var backupPath = string.Create(CultureInfo.InvariantCulture, $"{path}.legacy-{DateTime.UtcNow:yyyyMMddHHmmss}.bak");

        SqliteConnection.ClearAllPools();
        File.Move(path, backupPath);
        return backupPath;
    }
}
