// <copyright file="TenantConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FatturaFacile.Identity.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Tenant"/> entity.
/// </summary>
public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        _ = builder.ToTable("tenants");
        _ = builder.HasKey(t => t.Id);
        _ = builder.Property(t => t.Id).ValueGeneratedNever();

        _ = builder.Property(t => t.Name).HasMaxLength(150).IsRequired();
        _ = builder.Property(t => t.Slug).HasMaxLength(50).IsRequired();
        _ = builder.Property(t => t.IsActive).IsRequired();

        _ = builder.HasIndex(t => t.Slug).IsUnique();
    }
}
