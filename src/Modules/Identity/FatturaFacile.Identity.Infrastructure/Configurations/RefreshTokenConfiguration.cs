// <copyright file="RefreshTokenConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FatturaFacile.Identity.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="RefreshToken"/> entity.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        _ = builder.ToTable("refresh_tokens");
        _ = builder.HasKey(r => r.Id);
        _ = builder.Property(r => r.Id).ValueGeneratedNever();

        _ = builder.Property(r => r.TokenHash).HasMaxLength(128).IsRequired();
        _ = builder.Property(r => r.ReplacedByTokenHash).HasMaxLength(128);

        _ = builder.HasIndex(r => r.TokenHash).IsUnique();
        _ = builder.HasIndex(r => r.UserId);
    }
}
