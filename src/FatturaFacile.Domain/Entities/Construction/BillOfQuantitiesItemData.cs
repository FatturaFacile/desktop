// <copyright file="BillOfQuantitiesItemData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Data transfer object holding engineering quantities, tariff standard, and unit prices for a bill of quantities line item.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="SiteId">The associated construction site identifier.</param>
/// <param name="Code">The standardized regional tariff or NPK catalog code.</param>
/// <param name="Standard">The tariff classification standard (e.g., Regional or Swiss NPK).</param>
/// <param name="Desc">The technical specification description of the work.</param>
/// <param name="Uom">The unit of measure.</param>
/// <param name="Qty">The planned engineering work quantity.</param>
/// <param name="UnitPrice">The contractual or tariff unit rate.</param>
public sealed record BillOfQuantitiesItemData(
    Guid         TenantId,
    Guid         SiteId,
    string       Code,
    StandardType Standard,
    string       Desc,
    string       Uom,
    decimal      Qty,
    decimal      UnitPrice);
