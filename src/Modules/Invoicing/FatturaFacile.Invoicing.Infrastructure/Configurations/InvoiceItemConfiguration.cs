// <copyright file="InvoiceItemConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FatturaFacile.Invoicing.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="InvoiceItem"/> entity.
/// </summary>
public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        _ = builder.ToTable("invoice_items");

        _ = builder.HasKey(item => item.Id);
        _ = builder.Property(item => item.Id).ValueGeneratedNever();

        _ = builder.Property(item => item.Description)
           .HasMaxLength(1000)
           .IsRequired();

        _ = builder.Property(item => item.Quantity).HasPrecision(12, 4);
        _ = builder.Property(item => item.UnitPrice).HasPrecision(18, 4);
        _ = builder.Property(item => item.TaxRate).HasPrecision(5, 2);
        _ = builder.Property(item => item.DiscountPercentage).HasPrecision(5, 2);
        _ = builder.Property(item => item.LineTotal).HasPrecision(18, 2);
        _ = builder.Property(item => item.TaxAmount).HasPrecision(18, 2);

        _ = builder.HasIndex(item => item.InvoiceId);
        _ = builder.HasIndex(item => item.TenantId);
    }
}
