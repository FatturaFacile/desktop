// <copyright file="MesModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Mes;

/// <summary>
/// EF Core mapping of the MES operator board cache and outbox.
/// </summary>
public static class MesModelConfiguration
{
    /// <summary>
    /// Maps the board cache (one row per tenant and work centre) and the command outbox. The tenant filters are added by the
    /// database context itself, so they read the tenant service of each context instance.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureMes(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<LocalMesBoard>(b =>
        {
            _ = b.ToTable("mes_boards");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.WorkCenterId).HasColumnName("work_center_id").HasMaxLength(36);
            _ = b.Property(x => x.BoardJson).HasColumnName("board_json").IsRequired();
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.WorkCenterId }).IsUnique();
        });

        _ = modelBuilder.Entity<MesOutboxEntry>(b =>
        {
            _ = b.ToTable("mes_outbox");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.Action).HasColumnName("action").HasConversion<int>();
            _ = b.Property(x => x.OperationId).HasColumnName("operation_id").HasMaxLength(36);
            _ = b.Property(x => x.PhaseId).HasColumnName("phase_id").HasMaxLength(36);
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(36);
            _ = b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at");
            _ = b.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
            _ = b.Property(x => x.Attempts).HasColumnName("attempts");
            _ = b.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
            _ = b.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            _ = b.HasIndex(x => new { x.TenantId, x.Status, x.OccurredAtUtc });
        });
    }
}
