// <copyright file="AuditModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// EF Core mapping of the local audit queue table.
/// </summary>
public static class AuditModelConfiguration
{
    /// <summary>
    /// The properties covered by the record hash: they can never be modified after the entry was written.
    /// </summary>
    public static readonly IReadOnlyList<string> SignedProperties =
    [
        nameof(LocalAuditEntry.TenantId),
        nameof(LocalAuditEntry.SequenceNumber),
        nameof(LocalAuditEntry.PreviousHash),
        nameof(LocalAuditEntry.PayloadHash),
        nameof(LocalAuditEntry.RecordHash),
        nameof(LocalAuditEntry.PayloadJson),
        nameof(LocalAuditEntry.OccurredAt),
        nameof(LocalAuditEntry.EntityType),
        nameof(LocalAuditEntry.EntityId),
        nameof(LocalAuditEntry.Action),
        nameof(LocalAuditEntry.DeviceId),
    ];

    /// <summary>
    /// Maps the local_audit_queue table with tenant isolation.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="currentTenantService">The active tenant.</param>
    public static void ConfigureAudit(this ModelBuilder modelBuilder, ICurrentTenantService currentTenantService)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(currentTenantService);

        _ = modelBuilder.Entity<LocalAuditEntry>(b =>
        {
            _ = b.ToTable("local_audit_queue");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.SequenceNumber).HasColumnName("sequence_number");
            _ = b.Property(x => x.PreviousHash).HasColumnName("previous_hash").IsRequired().HasMaxLength(64);
            _ = b.Property(x => x.PayloadHash).HasColumnName("payload_hash").IsRequired().HasMaxLength(64);
            _ = b.Property(x => x.RecordHash).HasColumnName("record_hash").IsRequired().HasMaxLength(64);
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            _ = b.Property(x => x.EntityType).HasColumnName("entity_type").IsRequired().HasMaxLength(100);
            _ = b.Property(x => x.EntityId).HasColumnName("entity_id").IsRequired().HasMaxLength(100);
            _ = b.Property(x => x.Action).HasColumnName("action").IsRequired().HasMaxLength(100);
            _ = b.Property(x => x.DeviceId).HasColumnName("device_id").IsRequired().HasMaxLength(36);
            _ = b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            _ = b.Property(x => x.SyncedAt).HasColumnName("synced_at");
            _ = b.Property(x => x.ServerReceivedAt).HasColumnName("server_received_at");
            _ = b.Property(x => x.ServerReference).HasColumnName("server_reference").HasMaxLength(100);
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.SequenceNumber }).IsUnique();
            _ = b.HasIndex(x => new { x.TenantId, x.Status, x.SequenceNumber });
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
        });
    }
}
