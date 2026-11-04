// <copyright file="UserConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FatturaFacile.Identity.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="User"/> entity.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        _ = builder.ToTable("users");
        _ = builder.HasKey(u => u.Id);
        _ = builder.Property(u => u.Id).ValueGeneratedNever();

        _ = builder.Property(u => u.NormalizedEmail).HasMaxLength(128).IsRequired();
        _ = builder.Property(u => u.EncryptedEmail).HasMaxLength(500).IsRequired();
        _ = builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        _ = builder.Property(u => u.SecurityStamp).HasMaxLength(50).IsRequired();
        _ = builder.Property(u => u.PreferredLanguage).HasMaxLength(10).IsRequired();
        _ = builder.Property(u => u.IsActive).IsRequired();

        _ = builder.HasIndex(u => new { u.TenantId, u.NormalizedEmail }).IsUnique();
    }
}
