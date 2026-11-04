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
    private const int MaxTokenHashLength = 128;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.ToTable("refresh_tokens");
        _ = builder.HasKey(r => r.Id);
        _ = builder.Property(r => r.Id).ValueGeneratedNever();

        _ = builder.Property(r => r.TokenHash).HasMaxLength(MaxTokenHashLength).IsRequired();
        _ = builder.Property(r => r.ReplacedByTokenHash).HasMaxLength(MaxTokenHashLength);

        _ = builder.HasIndex(r => r.TokenHash).IsUnique();
        _ = builder.HasIndex(r => r.UserId);
    }
}
