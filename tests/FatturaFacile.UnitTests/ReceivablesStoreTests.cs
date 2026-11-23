// <copyright file="ReceivablesStoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Receivables;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the local receivables cache and outbox: round trip, replacement, trimming, ordering, idempotent enqueue and tenant isolation.
/// </summary>
public sealed class ReceivablesStoreTests : IDisposable
{
    private static readonly Guid TenantA = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);
    private static readonly DateTime Noon = new(2027, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly ReceivablesStore store;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesStoreTests"/> class.
    /// </summary>
    public ReceivablesStoreTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        store = new ReceivablesStore(db, tenant);
    }

    /// <summary>
    /// A cached answer comes back as stored, a second save replaces it and an unknown key finds nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Cache_RoundTripsAndReplaces()
    {
        await store.SaveCacheAsync("invoicing/receivables/aging", "{\"a\":1}", Noon, CancellationToken.None);
        await store.SaveCacheAsync("invoicing/receivables/aging", "{\"a\":2}", Noon.AddHours(1), CancellationToken.None);

        var found = await store.FindCacheAsync("invoicing/receivables/aging", CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal("{\"a\":2}", found.Json);
        Assert.Equal(Noon.AddHours(1), found.SyncedAtUtc);
        Assert.Equal(1, await db.Set<LocalReceivablesCache>().CountAsync());
        Assert.Null(await store.FindCacheAsync("unknown", CancellationToken.None));
    }

    /// <summary>
    /// Trimming keeps the most recent rows of a prefix and leaves the other keys alone.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task TrimCache_KeepsTheNewestRowsOfThePrefix()
    {
        for (var i = 0; i < 5; i++)
        {
            await store.SaveCacheAsync("invoicing/receivables/items?page=" + i.ToString(System.Globalization.CultureInfo.InvariantCulture), "{}", Noon.AddMinutes(i), CancellationToken.None);
        }

        await store.SaveCacheAsync("invoicing/receivables/aging", "{}", Noon.AddDays(-30), CancellationToken.None);

        await store.TrimCacheAsync("invoicing/receivables/items?", 2, CancellationToken.None);

        Assert.NotNull(await store.FindCacheAsync("invoicing/receivables/items?page=4", CancellationToken.None));
        Assert.NotNull(await store.FindCacheAsync("invoicing/receivables/items?page=3", CancellationToken.None));
        Assert.Null(await store.FindCacheAsync("invoicing/receivables/items?page=2", CancellationToken.None));
        Assert.Null(await store.FindCacheAsync("invoicing/receivables/items?page=0", CancellationToken.None));
        Assert.NotNull(await store.FindCacheAsync("invoicing/receivables/aging", CancellationToken.None));
    }

    /// <summary>
    /// Pending entries come oldest first, sent and dismissed ones drop out, and refused ones are listed as conflicts.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Outbox_ListsPendingOldestFirstAndConflicts()
    {
        var late = Entry(Noon.AddMinutes(10));
        var early = Entry(Noon);
        var sent = Entry(Noon.AddMinutes(1));
        var refused = Entry(Noon.AddMinutes(2));
        foreach (var entry in new[] { late, early, sent, refused })
        {
            _ = await store.EnqueueAsync(entry, CancellationToken.None);
        }

        sent.Status = ReceivablesOutboxStatus.Sent;
        refused.Status = ReceivablesOutboxStatus.Conflict;
        refused.LastError = "Closed";
        await store.SaveAsync(CancellationToken.None);

        var pending = await store.PendingAsync(CancellationToken.None);
        var conflicts = await store.ConflictsAsync(CancellationToken.None);

        Assert.Equal<Guid>([early.Id, late.Id], pending.Select(entry => entry.Id));
        Assert.Equal(refused.Id, Assert.Single(conflicts).Id);
        Assert.Equal("Closed", conflicts[0].LastError);
    }

    /// <summary>
    /// Queueing the same command twice keeps one row and returns the first entry, so a replay can never apply twice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Enqueue_SameIdempotencyKey_ReturnsTheExistingEntry()
    {
        var first = Entry(Noon);
        var duplicate = Entry(Noon.AddMinutes(1));
        duplicate.IdempotencyKey = first.IdempotencyKey;
        first.Status = ReceivablesOutboxStatus.Sent;

        var one = await store.EnqueueAsync(first, CancellationToken.None);
        var two = await store.EnqueueAsync(duplicate, CancellationToken.None);

        Assert.Same(one, two);
        Assert.Equal(1, await db.Set<ReceivablesOutboxEntry>().CountAsync());
        Assert.Equal(ReceivablesOutboxStatus.Sent, two.Status);
    }

    /// <summary>
    /// A tenant never sees the cache or the outbox of another tenant.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task TenantIsolation_HidesTheOtherTenantData()
    {
        await store.SaveCacheAsync("invoicing/receivables/aging", "{}", Noon, CancellationToken.None);
        _ = await store.EnqueueAsync(Entry(Noon), CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "CH", "CHF");

        Assert.Null(await store.FindCacheAsync("invoicing/receivables/aging", CancellationToken.None));
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    private static ReceivablesOutboxEntry Entry(DateTime occurredAtUtc) => new()
    {
        Action = ReceivablesAction.ApplyAllocations,
        Route = "invoicing/receivables/allocations",
        CustomerId = Guid.NewGuid(),
        Reference = "INV-1",
        OccurredAtUtc = occurredAtUtc,
    };
}
