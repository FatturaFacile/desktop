// <copyright file="FieldServiceTechnicianBundle.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The offline work bundle of a technician: the open work orders with their customers and equipment, and the van stock.
/// </summary>
/// <param name="ServerTime">The server time of the pull, to use as `since` of the next delta.</param>
/// <param name="TechnicianId">The technician.</param>
/// <param name="Name">The technician name.</param>
/// <param name="Skills">The skills.</param>
/// <param name="Items">The open work orders.</param>
/// <param name="VanStockProducts">The products on the van.</param>
public sealed record FieldServiceTechnicianBundle(DateTimeOffset ServerTime, string TechnicianId, string Name, IReadOnlyList<string> Skills, IReadOnlyList<FieldServiceBundleItem> Items, int VanStockProducts);
