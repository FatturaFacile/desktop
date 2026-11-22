// <copyright file="FieldServiceModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.FieldService;

/// <summary>
/// EF Core mapping of the Field Service cache and outbox.
/// </summary>
public static class FieldServiceModelConfiguration
{
    /// <summary>
    /// Maps the cache (one row per tenant, kind and key) and the outbox of the operations queued offline (unique per tenant and
    /// client identifier). The tenant filters are added by the database context itself, so they read the tenant service of each
    /// context instance.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureFieldService(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<LocalFieldServiceCache>(b =>
        {
            _ = b.ToTable("field_service_cache");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.Kind).HasColumnName("kind").IsRequired().HasMaxLength(40);
            _ = b.Property(x => x.Key).HasColumnName("cache_key").IsRequired().HasMaxLength(64);
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.Kind, x.Key }).IsUnique();
        });

        _ = modelBuilder.Entity<FieldServiceOutboxEntry>(b =>
        {
            _ = b.ToTable("field_service_outbox");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.Operation).HasColumnName("operation").HasConversion<int>();
            _ = b.Property(x => x.WorkOrderId).HasColumnName("work_order_id").HasMaxLength(36);
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.ClientUuid).HasColumnName("client_uuid").IsRequired().HasMaxLength(64);
            _ = b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at");
            _ = b.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
            _ = b.Property(x => x.Attempts).HasColumnName("attempts");
            _ = b.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
            _ = b.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.ClientUuid }).IsUnique();
            _ = b.HasIndex(x => new { x.TenantId, x.Status, x.OccurredAtUtc });
        });
    }
}
