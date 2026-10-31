// <copyright file="ProductDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a product or service catalog item.
/// </summary>
/// <param name="Id">The unique product identifier.</param>
/// <param name="TenantId">The organizational tenant boundary identifier.</param>
/// <param name="Sku">The unique stock keeping unit identifier.</param>
/// <param name="Barcode">The optical barcode or GTIN string, if registered.</param>
/// <param name="Name">The commercial product title or description.</param>
/// <param name="Description">Detailed technical or descriptive information regarding the product.</param>
/// <param name="UnitMeasure">The unit of measure code (e.g., PCE, HUR, KGM).</param>
/// <param name="CostPrice">The tenant acquisition or production cost price in base currency.</param>
/// <param name="SalePrice">The default catalog sales price excluding value-added tax in base currency.</param>
/// <param name="TaxRate">The applicable standard value-added tax percentage rate.</param>
/// <param name="TrackInventory">A flag indicating whether warehouse stock inventory tracking is enabled.</param>
/// <param name="IsActive">A flag indicating whether the product is currently active and available for billing.</param>
public record ProductDto(
    Guid Id,
    Guid TenantId,
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    string UnitMeasure,
    decimal CostPrice,
    decimal SalePrice,
    decimal TaxRate,
    bool TrackInventory,
    bool IsActive);
