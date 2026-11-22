// <copyright file="FieldServicePartInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The consumption of a part from the van, as the technician entered it.
/// </summary>
/// <param name="WorkOrderId">The work order.</param>
/// <param name="ProductId">The product.</param>
/// <param name="Description">The description shown in the work order.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="UnitPrice">The list unit price.</param>
/// <param name="LotNumber">The lot number, when the product is lot tracked.</param>
public sealed record FieldServicePartInput(Guid WorkOrderId, Guid ProductId, string Description, decimal Quantity, decimal UnitPrice, string? LotNumber);
