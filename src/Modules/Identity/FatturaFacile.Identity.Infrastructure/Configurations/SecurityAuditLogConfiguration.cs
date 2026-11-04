// <copyright file="SecurityAuditLogConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FatturaFacile.Identity.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="SecurityAuditLog"/> entity.
/// </summary>
public sealed class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        _ = builder.ToTable("security_audit_logs");
        _ = builder.HasKey(s => s.Id);
        _ = builder.Property(s => s.Id).ValueGeneratedNever();

        _ = builder.Property(s => s.EventType).HasMaxLength(100).IsRequired();
        _ = builder.Property(s => s.IpAddress).HasMaxLength(50);
        _ = builder.Property(s => s.UserAgent).HasMaxLength(255);

        _ = builder.HasIndex(s => s.TimestampUtc);
        _ = builder.HasIndex(s => s.TenantId);
        _ = builder.HasIndex(s => s.UserId);
    }
}
