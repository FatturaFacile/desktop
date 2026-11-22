// <copyright file="HelpdeskModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Helpdesk;

/// <summary>
/// EF Core mapping of the helpdesk cache and outbox tables.
/// </summary>
public static class HelpdeskModelConfiguration
{
    /// <summary>
    /// Maps the helpdesk entities.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureHelpdesk(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<LocalHelpdeskCache>(b =>
        {
            _ = b.ToTable("helpdesk_cache");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.CacheKey).HasColumnName("cache_key").HasMaxLength(300).IsRequired();
            _ = b.Property(x => x.Json).HasColumnName("json").IsRequired();
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.CacheKey }).IsUnique();
        });

        _ = modelBuilder.Entity<HelpdeskOutboxEntry>(b =>
        {
            _ = b.ToTable("helpdesk_outbox");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.Action).HasColumnName("action").HasConversion<int>();
            _ = b.Property(x => x.TicketId).HasColumnName("ticket_id").HasMaxLength(36);
            _ = b.Property(x => x.TicketNumber).HasColumnName("ticket_number").HasMaxLength(64).IsRequired();
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(36);
            _ = b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at");
            _ = b.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
            _ = b.Property(x => x.Attempts).HasColumnName("attempts");
            _ = b.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
            _ = b.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
            _ = b.Property(x => x.ResultJson).HasColumnName("result_json");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            _ = b.HasIndex(x => new { x.TenantId, x.Status, x.OccurredAtUtc });
        });
    }
}
