// <copyright file="BackupTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests for the password-protected backup and restore.
/// </summary>
public sealed class BackupTests
{
    /// <summary>
    /// Verifies backup, wrong-password rejection and restore of database, key and data folder files.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task BackupAndRestore_RoundTripDatabaseAndKey()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"ff-backup-test-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(folder);
        var paths = new DatabasePaths(Path.Combine(folder, "db.sqlite"), Path.Combine(folder, "field.key"));
        var zipPath = Path.Combine(folder, "backup.zip");

        try
        {
            await ExecuteAsync(paths.DatabasePath, "CREATE TABLE t(v TEXT); INSERT INTO t VALUES('originale');");
            var originalKey = DpapiKeyStore.LoadOrCreate(paths.KeyPath);
            var template = Path.Combine(folder, "Templates", "Invoice", "Fattura_IT.docx");
            _ = Directory.CreateDirectory(Path.GetDirectoryName(template)!);
            await File.WriteAllTextAsync(template, "template originale");
            var service = new ZipBackupService(paths);

            await service.CreateBackupAsync(zipPath, "password-backup-1");

            await ExecuteAsync(paths.DatabasePath, "UPDATE t SET v = 'modificato';");
            DpapiKeyStore.Save(paths.KeyPath, new byte[32]);
            await File.WriteAllTextAsync(template, "template modificato");

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RestoreBackupAsync(zipPath, "password-sbagliata"));

            var safetyCopy = await service.RestoreBackupAsync(zipPath, "password-backup-1");

            Assert.True(File.Exists(safetyCopy));
            Assert.Equal("originale", await ScalarAsync(paths.DatabasePath, "SELECT v FROM t;"));
            Assert.Equal(originalKey, DpapiKeyStore.LoadOrCreate(paths.KeyPath));
            Assert.Equal("template originale", await File.ReadAllTextAsync(template));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(folder, recursive: true);
        }
    }

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Test helper with constant SQL.")]
    private static async Task ExecuteAsync(string databasePath, string sql)
    {
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        _ = await command.ExecuteNonQueryAsync();
    }

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Test helper with constant SQL.")]
    private static async Task<object?> ScalarAsync(string databasePath, string sql)
    {
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return await command.ExecuteScalarAsync();
    }
}
