// <copyright file="AnalyticalModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.AnalyticalAccounting;

/// <summary>
/// EF Core mapping of the analytical cost accounting offline cache and queue (Mod 018).
/// </summary>
public static class AnalyticalModelConfiguration
{
    /// <summary>
    /// Maps the snapshot cache (one row per tenant and read model) and the offline operation queue. The tenant filters are added by
    /// the database context itself, so they read the tenant service of each context instance.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureAnalyticalAccounting(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<LocalAnalyticalSnapshot>(b =>
        {
            _ = b.ToTable("analytical_snapshots");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.Key).HasColumnName("snapshot_key").IsRequired().HasMaxLength(40);
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        });

        _ = modelBuilder.Entity<LocalAnalyticalOperation>(b =>
        {
            _ = b.ToTable("analytical_operations");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(16);
            _ = b.Property(x => x.Attempts).HasColumnName("attempts");
            _ = b.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(500);
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.Status });
        });
    }
}
