// <copyright file="AuditTestDatabase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>
#pragma warning disable SA1600 // Elements should be documented: internal test double.

using System.IO;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.UnitTests;

/// <summary>
/// An in-memory SQLite database with the real context, interceptors, key provider and audit queue.
/// </summary>
internal sealed class AuditTestDatabase : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly string directory = Path.Combine(Path.GetTempPath(), "ff-audit-" + Guid.NewGuid().ToString("N"));

    public AuditTestDatabase()
    {
        connection.Open();
        _ = Directory.CreateDirectory(directory);
        Keys = new AuditDeviceKeyProvider(new FakeKeyProtector(), Path.Combine(directory, "audit.key"));

        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(Tenant))
            .Options;
        Db = new FatturaFacileDbContext(options, Tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = Db.Database.EnsureCreated();
        Queue = new LocalAuditQueue(Db, Tenant, Keys, Clock);
    }

    public CurrentTenantService Tenant { get; } = new();

    public ManualTimeProvider Clock { get; } = new();

    public AuditDeviceKeyProvider Keys { get; }

    public FatturaFacileDbContext Db { get; }

    public LocalAuditQueue Queue { get; }

    public void Dispose()
    {
        Db.Dispose();
        connection.Dispose();

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
