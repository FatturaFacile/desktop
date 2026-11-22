// <copyright file="FieldServiceStoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.FieldService;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the local Field Service cache and outbox: round trip, replacement, tenant isolation, ordering of the outbox
/// and the uniqueness of the client identifier that makes a replay harmless.
/// </summary>
public sealed class FieldServiceStoreTests : IDisposable
{
    private static readonly Guid TenantA = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);
    private static readonly DateTime Noon = new(2027, 3, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly FieldServiceStore store;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceStoreTests"/> class.
    /// </summary>
    public FieldServiceStoreTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        store = new FieldServiceStore(db, tenant);
    }

    /// <summary>
    /// A saved payload comes back as it was stored, with its timestamp.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveAndFind_RoundTripsThePayload()
    {
        await store.SaveAsync(FieldServiceCacheKinds.Board, FieldServiceCacheKinds.All, FieldServiceTestData.Board, Noon, CancellationToken.None);

        var stored = await store.FindAsync(FieldServiceCacheKinds.Board, FieldServiceCacheKinds.All, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(FieldServiceTestData.Board, stored.Json);
        Assert.Equal(Noon, stored.SyncedAtUtc);
        Assert.Null(await store.FindAsync(FieldServiceCacheKinds.Contracts, FieldServiceCacheKinds.All, CancellationToken.None));
    }

    /// <summary>
    /// Saving again replaces the row of the same kind and key, and keys of one kind are kept apart.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_ReplacesTheSameKindAndKeyOnly()
    {
        await store.SaveAsync(FieldServiceCacheKinds.WorkOrder, FieldServiceTestData.OrderId, "{\"v\":1}", Noon, CancellationToken.None);
        await store.SaveAsync(FieldServiceCacheKinds.WorkOrder, FieldServiceTestData.OtherOrderId, "{\"v\":2}", Noon, CancellationToken.None);
        await store.SaveAsync(FieldServiceCacheKinds.WorkOrder, FieldServiceTestData.OrderId, "{\"v\":3}", Noon.AddHours(1), CancellationToken.None);

        Assert.Equal(2, await db.Set<LocalFieldServiceCache>().CountAsync());
        var first = await store.FindAsync(FieldServiceCacheKinds.WorkOrder, FieldServiceTestData.OrderId, CancellationToken.None);
        Assert.Equal("{\"v\":3}", first?.Json);
        Assert.Equal(Noon.AddHours(1), first?.SyncedAtUtc);
        Assert.Equal("{\"v\":2}", (await store.FindAsync(FieldServiceCacheKinds.WorkOrder, FieldServiceTestData.OtherOrderId, CancellationToken.None))?.Json);
    }

    /// <summary>
    /// A tenant never sees the cache or the outbox of another tenant.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Tenants_AreIsolated()
    {
        await store.SaveAsync(FieldServiceCacheKinds.Contracts, FieldServiceCacheKinds.All, "[]", Noon, CancellationToken.None);
        await store.EnqueueAsync(FieldServiceTestData.Entry(FieldServiceOperationType.StatusTransition, FieldServiceTestData.OrderId, "{}", Noon), CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "IT", "EUR");

        Assert.Null(await store.FindAsync(FieldServiceCacheKinds.Contracts, FieldServiceCacheKinds.All, CancellationToken.None));
        Assert.Empty(await store.PendingAsync(CancellationToken.None));

        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");

        Assert.NotNull(await store.FindAsync(FieldServiceCacheKinds.Contracts, FieldServiceCacheKinds.All, CancellationToken.None));
        _ = Assert.Single(await store.PendingAsync(CancellationToken.None));
    }

    /// <summary>
    /// The outbox lists the pending operations in the order the technician acted, and the conflicts apart.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Outbox_ListsPendingInTheOrderOfTheAction()
    {
        var late = FieldServiceTestData.Entry(FieldServiceOperationType.ServiceReport, FieldServiceTestData.OrderId, "{}", Noon.AddMinutes(30));
        var early = FieldServiceTestData.Entry(FieldServiceOperationType.PartConsumption, FieldServiceTestData.OrderId, "{}", Noon);
        var refused = FieldServiceTestData.Entry(FieldServiceOperationType.PartConsumption, FieldServiceTestData.OrderId, "{}", Noon.AddMinutes(10));
        refused.Status = FieldServiceOutboxStatus.Conflict;
        await store.EnqueueAsync(late, CancellationToken.None);
        await store.EnqueueAsync(early, CancellationToken.None);
        await store.EnqueueAsync(refused, CancellationToken.None);

        var pending = await store.PendingAsync(CancellationToken.None);
        var conflicts = await store.ConflictsAsync(CancellationToken.None);

        Assert.Equal([early.Id, late.Id], pending.Select(e => e.Id));
        Assert.Equal(refused.Id, Assert.Single(conflicts).Id);
    }

    /// <summary>
    /// Two operations with the same client identifier cannot both be stored, so a replayed enqueue is refused by the database.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Outbox_RefusesADuplicateClientIdentifier()
    {
        var first = FieldServiceTestData.Entry(FieldServiceOperationType.PartConsumption, FieldServiceTestData.OrderId, "{}", Noon);
        var second = FieldServiceTestData.Entry(FieldServiceOperationType.PartConsumption, FieldServiceTestData.OrderId, "{}", Noon);
        second.ClientUuid = first.ClientUuid;
        await store.EnqueueAsync(first, CancellationToken.None);

        _ = await Assert.ThrowsAsync<DbUpdateException>(() => store.EnqueueAsync(second, CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }
}
