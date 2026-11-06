// <copyright file="CreateProductCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Products.Commands;

/// <summary>
/// Command to create a new product item or service.
/// </summary>
public sealed class CreateProductCommand : IRequest<ProductDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;

    /// <summary>
    /// Gets or sets the SKU code.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets barcode.
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Gets or sets product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets unit measure (e.g. PZ, EA, KG, ORE).
    /// </summary>
    public string UnitMeasure { get; set; } = "PZ";

    /// <summary>
    /// Gets or sets cost price.
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Gets or sets sales price.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Gets or sets VAT rate percentage.
    /// </summary>
    public decimal TaxRate { get; set; } = 22m;

    /// <summary>
    /// Gets or sets HS code.
    /// </summary>
    public string? HsCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether stock inventory is tracked.
    /// </summary>
    public bool TrackInventory { get; set; } = true;
}
