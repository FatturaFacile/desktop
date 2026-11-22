// <copyright file="AnalyticalStoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.AnalyticalAccounting;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the local analytical cost accounting cache and offline queue: round trip, replacement, queue order and tenant
/// isolation.
/// </summary>
public sealed class AnalyticalStoreTests : IDisposable
{
    private static readonly Guid TenantA = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly AnalyticalSnapshotStore store;
    private readonly AnalyticalOperationQueue queue;
    private DateTimeOffset now = new(2027, 3, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticalStoreTests"/> class.
    /// </summary>
    public AnalyticalStoreTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        store = new AnalyticalSnapshotStore(db, tenant);
        queue = new AnalyticalOperationQueue(db, tenant, new MovingTime(() => now));
    }

    /// <summary>
    /// A saved read model comes back as it was stored, and a second save replaces it in the same row.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveAndFind_RoundTripsAndReplaces()
    {
        var first = new DateTime(2027, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var second = first.AddHours(1);

        await store.SaveAsync("margin", "{\"v\":1}", first, CancellationToken.None);
        await store.SaveAsync("margin", "{\"v\":2}", second, CancellationToken.None);
        var stored = await store.FindAsync("margin", CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal("{\"v\":2}", stored.Json);
        Assert.Equal(second, stored.SyncedAtUtc);
        Assert.Equal(1, await db.Set<LocalAnalyticalSnapshot>().CountAsync());
        Assert.Null(await store.FindAsync("runs", CancellationToken.None));
    }

    /// <summary>
    /// A tenant never sees the read models of another one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Find_OtherTenant_SeesNothing()
    {
        await store.SaveAsync("centers", "[]", DateTime.UtcNow, CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "CH", "CHF");

        Assert.Null(await store.FindAsync("centers", CancellationToken.None));
    }

    /// <summary>
    /// Queued entries come out oldest first, an accepted one leaves the pending list and a refused one is parked with its reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queue_KeepsOrderAndMarksTheOutcome()
    {
        var first = await queue.EnqueueAsync("{\"n\":1}", CancellationToken.None);
        now = now.AddMinutes(1);
        var second = await queue.EnqueueAsync("{\"n\":2}", CancellationToken.None);

        var pending = await queue.PendingAsync(CancellationToken.None);
        Assert.Equal([first, second], pending.Select(p => p.Id));
        Assert.Equal(2, await queue.CountPendingAsync(CancellationToken.None));

        await queue.MarkSentAsync(first, CancellationToken.None);
        await queue.MarkFailedAsync(second, "Period consolidated", CancellationToken.None);

        Assert.Equal(0, await queue.CountPendingAsync(CancellationToken.None));
        var parked = await db.Set<LocalAnalyticalOperation>().AsNoTracking().SingleAsync(o => o.Id == second);
        Assert.Equal(LocalAnalyticalOperation.Failed, parked.Status);
        Assert.Equal("Period consolidated", parked.LastError);
        Assert.Equal(1, parked.Attempts);
    }

    /// <summary>
    /// Another tenant neither sees nor counts the queued entries.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queue_OtherTenant_SeesNothing()
    {
        _ = await queue.EnqueueAsync("{\"n\":1}", CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "CH", "CHF");

        Assert.Empty(await queue.PendingAsync(CancellationToken.None));
        Assert.Equal(0, await queue.CountPendingAsync(CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    private sealed class MovingTime(Func<DateTimeOffset> read) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => read();
    }
}
