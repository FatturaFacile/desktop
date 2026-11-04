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
    private const int MaxEventTypeLength = 100;
    private const int MaxIpAddressLength = 50;
    private const int MaxUserAgentLength = 255;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.ToTable("security_audit_logs");
        _ = builder.HasKey(s => s.Id);
        _ = builder.Property(s => s.Id).ValueGeneratedNever();

        _ = builder.Property(s => s.EventType).HasMaxLength(MaxEventTypeLength).IsRequired();
        _ = builder.Property(s => s.IpAddress).HasMaxLength(MaxIpAddressLength);
        _ = builder.Property(s => s.UserAgent).HasMaxLength(MaxUserAgentLength);

        _ = builder.HasIndex(s => s.TimestampUtc);
        _ = builder.HasIndex(s => s.TenantId);
        _ = builder.HasIndex(s => s.UserId);
    }
}
