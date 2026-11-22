// <copyright file="CustomerLifecycleModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.CustomerLifecycle;

/// <summary>
/// EF Core mapping of the customer quick action outbox.
/// </summary>
public static class CustomerLifecycleModelConfiguration
{
    /// <summary>
    /// Maps the customer lifecycle entities.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureCustomerLifecycle(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<CustomerActivityOutboxEntry>(b =>
        {
            _ = b.ToTable("customer_activity_outbox");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.AccountId).HasColumnName("account_id").HasMaxLength(36);
            _ = b.Property(x => x.Type).HasColumnName("type").HasMaxLength(32).IsRequired();
            _ = b.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(200).IsRequired();
            _ = b.Property(x => x.DueDate).HasColumnName("due_date");
            _ = b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at");
            _ = b.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
            _ = b.Property(x => x.Attempts).HasColumnName("attempts");
            _ = b.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
            _ = b.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.Status, x.OccurredAtUtc });
        });
    }
}
