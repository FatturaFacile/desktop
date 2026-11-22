// <copyright file="ProjectEvmSnapshotStoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.ProjectEvm;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the local project profitability cache: round trip, one row per project, replacement and tenant isolation.
/// </summary>
public sealed class ProjectEvmSnapshotStoreTests : IDisposable
{
    private static readonly Guid TenantA = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly ProjectEvmSnapshotStore store;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectEvmSnapshotStoreTests"/> class.
    /// </summary>
    public ProjectEvmSnapshotStoreTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        store = new ProjectEvmSnapshotStore(db, tenant);
    }

    /// <summary>
    /// A saved snapshot comes back as it was stored, with its timestamp.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveAndFind_RoundTripsThePayload()
    {
        var at = new DateTime(2027, 3, 1, 12, 0, 0, DateTimeKind.Utc);

        await store.SaveAsync(ProjectEvmTestData.ProjectId, ProjectEvmTestData.Payload(), at, CancellationToken.None);
        var stored = await store.FindAsync(ProjectEvmTestData.ProjectId, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(at, stored.SyncedAtUtc);
        Assert.Equal(ProjectEvmTestData.Breakdown, stored.Payload.BreakdownJson);
        Assert.Equal(ProjectEvmTestData.Waterfall, stored.Payload.WaterfallJson);
        Assert.Equal(ProjectEvmTestData.Scurve, stored.Payload.ScurveJson);
    }

    /// <summary>
    /// Nothing is found for a project that was never pulled.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Find_BeforeTheFirstSync_ReturnsNull()
    {
        Assert.Null(await store.FindAsync(ProjectEvmTestData.ProjectId, CancellationToken.None));
        Assert.Null(await store.FindProjectsAsync(CancellationToken.None));
    }

    /// <summary>
    /// A second save replaces the row of the project instead of adding one, and the picker has a row of its own.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Twice_KeepsOneRowPerProject()
    {
        var first = new DateTime(2027, 3, 1, 12, 0, 0, DateTimeKind.Utc);

        await store.SaveAsync(ProjectEvmTestData.ProjectId, ProjectEvmTestData.Payload(), first, CancellationToken.None);
        await store.SaveAsync(ProjectEvmTestData.ProjectId, new ProjectEvmPayload("{}", "{}", "{}"), first.AddDays(1), CancellationToken.None);
        await store.SaveProjectsAsync(ProjectEvmTestData.Projects, first, CancellationToken.None);

        Assert.Equal(2, await db.Set<LocalProjectEvmSnapshot>().CountAsync());
        var stored = await store.FindAsync(ProjectEvmTestData.ProjectId, CancellationToken.None);
        Assert.Equal("{}", stored?.Payload.BreakdownJson);
        Assert.Equal(first.AddDays(1), stored?.SyncedAtUtc);
        Assert.Equal(ProjectEvmTestData.Projects, await store.FindProjectsAsync(CancellationToken.None));
    }

    /// <summary>
    /// A tenant never sees the snapshots of another one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Tenants_AreIsolated()
    {
        await store.SaveAsync(ProjectEvmTestData.ProjectId, ProjectEvmTestData.Payload(), DateTime.UtcNow, CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "CH", "CHF");

        Assert.Null(await store.FindAsync(ProjectEvmTestData.ProjectId, CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }
}
