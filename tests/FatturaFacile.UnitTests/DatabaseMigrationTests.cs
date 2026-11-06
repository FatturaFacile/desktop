// <copyright file="DatabaseMigrationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Cryptography;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Integration tests on a real SQLite file: migrations, seeding, legacy backup and column encryption at rest.
/// </summary>
public sealed class DatabaseMigrationTests
{
    /// <summary>
    /// Verifies that migrations create the schema, the seed runs and personal data is encrypted on disk.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Migrate_CreatesSchemaSeedsAndEncryptsPersonalData()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ff-test-{Guid.NewGuid():N}.sqlite");

        try
        {
            await using (var context = CreateContext(path))
            {
                await DatabaseSeeder.SeedAsync(context);

                Assert.Contains("20261105202128_InitialCreate", await context.Database.GetAppliedMigrationsAsync(), StringComparer.Ordinal);
                Assert.Equal(2, await context.Tenants.CountAsync());
            }

            await using var connection = new SqliteConnection($"Data Source={path}");
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT social_security_number FROM hr_employees LIMIT 1";
            var stored = (string?)await command.ExecuteScalarAsync();

            Assert.StartsWith(AesGcmFieldEncryptor.Prefix, stored, StringComparison.Ordinal);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            DeleteDatabaseFiles(path);
        }
    }

    /// <summary>
    /// Verifies that a database created without migrations is backed up and replaced by a migrated one.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Migrate_BacksUpLegacyEnsureCreatedDatabase()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ff-legacy-{Guid.NewGuid():N}.sqlite");
        string? backup = null;

        try
        {
            await using (var legacy = CreateContext(path))
            {
                _ = await legacy.Database.EnsureCreatedAsync();
            }

            await using (var context = CreateContext(path))
            {
                backup = await DatabaseMigrator.MigrateAsync(context);
                Assert.NotEmpty(await context.Database.GetAppliedMigrationsAsync());
            }

            Assert.NotNull(backup);
            Assert.True(File.Exists(backup));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            DeleteDatabaseFiles(path);

            if (backup is not null)
            {
                File.Delete(backup);
            }
        }
    }

    private static FatturaFacileDbContext CreateContext(string path)
    {
        var tenantService = new CurrentTenantService();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite($"Data Source={path}")
            .AddInterceptors(new TenantSaveChangesInterceptor(tenantService))
            .Options;

        return new FatturaFacileDbContext(options, tenantService, new AesGcmFieldEncryptor(RandomNumberGenerator.GetBytes(32)));
    }

    private static void DeleteDatabaseFiles(string path)
    {
        foreach (var file in new[] { path, path + "-wal", path + "-shm" })
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
