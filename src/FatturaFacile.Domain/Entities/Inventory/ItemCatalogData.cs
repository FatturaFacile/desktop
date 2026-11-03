// <copyright file="ItemCatalogData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Data transfer object holding pricing, taxation, and categorization attributes for a catalog item.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="Code">The internal SKU or item reference code.</param>
/// <param name="Description">The commercial product description.</param>
/// <param name="Category">The product grouping category.</param>
/// <param name="Uom">The unit of measure (e.g., PZ, KG, MT).</param>
/// <param name="UnitCost">The baseline unit acquisition or production cost.</param>
/// <param name="SalePrice">The standard unit sales price excluding tax.</param>
/// <param name="Currency">The pricing currency denomination.</param>
/// <param name="VatRate">The applicable statutory VAT percentage.</param>
/// <param name="Barcode">The optional EAN/UPC barcode string.</param>
public sealed record ItemCatalogData(
    Guid     TenantId,
    string   Code,
    string   Description,
    string   Category,
    string   Uom,
    decimal  UnitCost,
    decimal  SalePrice,
    Currency Currency,
    decimal  VatRate = 22.0m,
    string?  Barcode = null);
