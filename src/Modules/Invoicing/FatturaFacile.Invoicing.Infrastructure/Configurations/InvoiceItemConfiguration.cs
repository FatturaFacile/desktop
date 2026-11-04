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
    private const int MaxDescriptionLength = 1000;
    private const int QuantityPrecision = 12;
    private const int QuantityScale = 4;
    private const int UnitPricePrecision = 18;
    private const int UnitPriceScale = 4;
    private const int PercentagePrecision = 5;
    private const int PercentageScale = 2;
    private const int MonetaryPrecision = 18;
    private const int MonetaryScale = 2;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.ToTable("invoice_items");

        _ = builder.HasKey(item => item.Id);
        _ = builder.Property(item => item.Id).ValueGeneratedNever();

        _ = builder.Property(item => item.Description)
           .HasMaxLength(MaxDescriptionLength)
           .IsRequired();

        _ = builder.Property(item => item.Quantity).HasPrecision(QuantityPrecision, QuantityScale);
        _ = builder.Property(item => item.UnitPrice).HasPrecision(UnitPricePrecision, UnitPriceScale);
        _ = builder.Property(item => item.TaxRate).HasPrecision(PercentagePrecision, PercentageScale);
        _ = builder.Property(item => item.DiscountPercentage).HasPrecision(PercentagePrecision, PercentageScale);
        _ = builder.Property(item => item.LineTotal).HasPrecision(MonetaryPrecision, MonetaryScale);
        _ = builder.Property(item => item.TaxAmount).HasPrecision(MonetaryPrecision, MonetaryScale);

        _ = builder.HasIndex(item => item.InvoiceId);
        _ = builder.HasIndex(item => item.TenantId);
    }
}
