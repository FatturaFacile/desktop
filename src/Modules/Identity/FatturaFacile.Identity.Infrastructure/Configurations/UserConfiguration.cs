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
    private const int MaxNormalizedEmailLength = 128;
    private const int MaxEncryptedEmailLength = 500;
    private const int MaxPasswordHashLength = 255;
    private const int MaxSecurityStampLength = 50;
    private const int MaxPreferredLanguageLength = 10;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.ToTable("users");
        _ = builder.HasKey(u => u.Id);
        _ = builder.Property(u => u.Id).ValueGeneratedNever();

        _ = builder.Property(u => u.NormalizedEmail).HasMaxLength(MaxNormalizedEmailLength).IsRequired();
        _ = builder.Property(u => u.EncryptedEmail).HasMaxLength(MaxEncryptedEmailLength).IsRequired();
        _ = builder.Property(u => u.PasswordHash).HasMaxLength(MaxPasswordHashLength).IsRequired();
        _ = builder.Property(u => u.SecurityStamp).HasMaxLength(MaxSecurityStampLength).IsRequired();
        _ = builder.Property(u => u.PreferredLanguage).HasMaxLength(MaxPreferredLanguageLength).IsRequired();
        _ = builder.Property(u => u.IsActive).IsRequired();

        _ = builder.HasIndex(u => new { u.TenantId, u.NormalizedEmail }).IsUnique();
    }
}
