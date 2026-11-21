// <copyright file="SyncModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// EF Core mapping of the sync tables.
/// </summary>
public static class SyncModelConfiguration
{
    /// <summary>
    /// Maps the outbox, checkpoint and record state tables with tenant isolation.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="currentTenantService">The active tenant.</param>
    public static void ConfigureSync(this ModelBuilder modelBuilder, ICurrentTenantService currentTenantService)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(currentTenantService);

        _ = modelBuilder.Entity<SyncOutboxEntry>(b =>
        {
            _ = b.ToTable("sync_outbox");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.EntityName).HasColumnName("entity_name").IsRequired().HasMaxLength(100);
            _ = b.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(36);
            _ = b.Property(x => x.Operation).HasColumnName("operation").HasConversion<string>().HasMaxLength(20);
            _ = b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            _ = b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(36);
            _ = b.Property(x => x.Attempts).HasColumnName("attempts");
            _ = b.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
            _ = b.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(1000);
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAt });
            _ = b.HasIndex(x => new { x.TenantId, x.EntityName, x.EntityId });
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<SyncCheckpoint>(b =>
        {
            _ = b.ToTable("sync_checkpoints");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.EntityName).HasColumnName("entity_name").IsRequired().HasMaxLength(100);
            _ = b.Property(x => x.LastServerTime).HasColumnName("last_server_time");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.EntityName }).IsUnique();
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<SyncEntityState>(b =>
        {
            _ = b.ToTable("sync_entity_states");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.EntityName).HasColumnName("entity_name").IsRequired().HasMaxLength(100);
            _ = b.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(36);
            _ = b.Property(x => x.SyncStatus).HasColumnName("sync_status").HasConversion<string>().HasMaxLength(20);
            _ = b.Property(x => x.ServerVersion).HasColumnName("server_version").HasMaxLength(64);
            _ = b.Property(x => x.LastSyncedAt).HasColumnName("last_synced_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.EntityName, x.EntityId }).IsUnique();
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
        });
    }
}
