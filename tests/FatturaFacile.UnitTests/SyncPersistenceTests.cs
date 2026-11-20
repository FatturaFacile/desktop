// <copyright file="SyncPersistenceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using FatturaFacile.Infrastructure.Sync;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of outbox capture, applying pulled data and payload building.
/// </summary>
public sealed class SyncPersistenceTests : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncPersistenceTests"/> class.
    /// </summary>
    public SyncPersistenceTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant), new SyncChangeCaptureInterceptor(TimeProvider.System))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
    }

    /// <summary>
    /// A new contact is queued as a create together with a pending record state.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AddContact_QueuesCreateAndPendingState()
    {
        var contact = NewContact();
        _ = db.Contacts.Add(contact);
        _ = await db.SaveChangesAsync();

        var entry = await db.SyncOutbox.SingleAsync();
        Assert.Equal(SyncOperation.Create, entry.Operation);
        Assert.Equal(contact.Id, entry.EntityId);
        Assert.Equal(SyncOutboxStatus.Pending, entry.Status);
        Assert.Equal(SyncOutboxStatus.Pending, (await db.SyncEntityStates.SingleAsync()).SyncStatus);
    }

    /// <summary>
    /// Edits of a record that still has a pending write do not pile up duplicate entries.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task UpdateContact_WithPendingCreate_DoesNotQueueAgain()
    {
        var contact = NewContact();
        _ = db.Contacts.Add(contact);
        _ = await db.SaveChangesAsync();

        contact.City = "Bern";
        _ = await db.SaveChangesAsync();

        Assert.Equal(1, await db.SyncOutbox.CountAsync());
    }

    /// <summary>
    /// After the create was delivered, an edit is queued as an update and a logical delete as a delete.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task UpdateAndDelete_AfterDelivery_AreQueued()
    {
        var contact = NewContact();
        _ = db.Contacts.Add(contact);
        _ = await db.SaveChangesAsync();
        (await db.SyncOutbox.SingleAsync()).Status = SyncOutboxStatus.Synced;
        _ = await db.SaveChangesAsync();

        contact.City = "Bern";
        _ = await db.SaveChangesAsync();
        contact.DeletedAt = DateTime.UtcNow;
        _ = await db.SaveChangesAsync();

        var operations = await db.SyncOutbox.OrderBy(e => e.CreatedAt).Select(e => e.Operation).ToListAsync();
        Assert.Contains(SyncOperation.Update, operations);
        Assert.Equal(SyncOperation.Delete, operations[^1]);
    }

    /// <summary>
    /// Pulled data is written locally without echoing it back through the outbox, and records server version.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Apply_InsertsAndUpdatesWithoutOutbox()
    {
        var handler = CreateHandler();
        var id = Guid.NewGuid();

        _ = await handler.ApplyAsync([Row(id, "Muster AG", "2026-11-20T10:00:00Z")], [], new HashSet<Guid>(), CancellationToken.None);
        var result = await handler.ApplyAsync([Row(id, "Muster SA", "2026-11-20T11:00:00Z")], [], new HashSet<Guid>(), CancellationToken.None);

        var contact = await db.Contacts.SingleAsync();
        Assert.Equal("Muster SA", contact.CompanyName);
        Assert.Equal(tenant.TenantId, contact.TenantId);
        Assert.Equal(1, result.Applied);
        Assert.Empty(db.SyncOutbox);
        var state = await db.SyncEntityStates.SingleAsync();
        Assert.Equal(SyncOutboxStatus.Synced, state.SyncStatus);
        Assert.Equal("2026-11-20T11:00:00Z", state.ServerVersion);
    }

    /// <summary>
    /// A pulled change never overwrites a record with a pending local edit.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Apply_SkipsProtectedRecords()
    {
        var contact = NewContact();
        _ = db.Contacts.Add(contact);
        _ = await db.SaveChangesAsync();

        var result = await CreateHandler().ApplyAsync(
            [Row(contact.Id, "Server Name", "2026-11-20T10:00:00Z")],
            [],
            new HashSet<Guid> { contact.Id },
            CancellationToken.None);

        Assert.Equal(1, result.Skipped);
        Assert.Equal("Muster AG", (await db.Contacts.SingleAsync()).CompanyName);
    }

    /// <summary>
    /// A record deleted on the server is deleted logically and not queued back.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Apply_ServerDelete_SoftDeletesLocally()
    {
        var handler = CreateHandler();
        var id = Guid.NewGuid();
        _ = await handler.ApplyAsync([Row(id, "Muster AG", "2026-11-20T10:00:00Z")], [], new HashSet<Guid>(), CancellationToken.None);

        _ = await handler.ApplyAsync([], [id], new HashSet<Guid>(), CancellationToken.None);

        _ = Assert.NotNull((await db.Contacts.SingleAsync()).DeletedAt);
        Assert.Empty(db.SyncOutbox);
    }

    /// <summary>
    /// Malformed server rows are skipped instead of failing the pass.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Apply_MalformedRow_IsSkipped()
    {
        using var bad = JsonDocument.Parse("""{"id":"not-a-guid"}""");

        var result = await CreateHandler().ApplyAsync([bad.RootElement.Clone()], [], new HashSet<Guid>(), CancellationToken.None);

        Assert.Equal(1, result.Skipped);
        Assert.Empty(db.Contacts);
    }

    /// <summary>
    /// The write payload uses the server naming and leaves out the local tenant id.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task BuildPayload_UsesSnakeCaseWithoutTenant()
    {
        var contact = NewContact();
        _ = db.Contacts.Add(contact);
        _ = await db.SaveChangesAsync();

        var payload = await CreateHandler().BuildPayloadAsync(contact.Id, CancellationToken.None);

        Assert.NotNull(payload);
        using var json = JsonDocument.Parse(payload);
        Assert.Equal("Muster AG", json.RootElement.GetProperty("company_name").GetString());
        Assert.False(json.RootElement.TryGetProperty("tenant_id", out _));
    }

    /// <summary>
    /// The checkpoint is stored per entity and returned on the next pass.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Store_CheckpointRoundTrips_AndProtectsPendingIds()
    {
        SyncStore store = new(db, tenant, TimeProvider.System);
        var time = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);

        Assert.Null(await store.GetCheckpointAsync("contacts", CancellationToken.None));
        await store.SaveCheckpointAsync("contacts", time, CancellationToken.None);
        await store.SaveCheckpointAsync("contacts", time.AddMinutes(5), CancellationToken.None);

        Assert.Equal(time.AddMinutes(5), await store.GetCheckpointAsync("contacts", CancellationToken.None));

        var contact = NewContact();
        _ = db.Contacts.Add(contact);
        _ = await db.SaveChangesAsync();
        Assert.Contains(contact.Id, await store.GetProtectedEntityIdsAsync("contacts", CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    private static JsonElement Row(Guid id, string company, string updatedAt)
    {
        using var document = JsonDocument.Parse(
            $$"""{"id":"{{id}}","company_name":"{{company}}","type":"client","country":"CH","updated_at":"{{updatedAt}}","extra_field":42}""");
        return document.RootElement.Clone();
    }

    private Contact NewContact() => new() { TenantId = tenant.TenantId, CompanyName = "Muster AG", Country = "CH" };

    private EfSyncEntityHandler<Contact> CreateHandler() => new(SyncCatalog.Contacts, SyncCatalog.Contacts, db, tenant, TimeProvider.System);
}
