// <copyright file="MesComponent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// A component of the production order, a choice of the lot registration.
/// </summary>
/// <param name="ItemId">The manufacturing item identifier.</param>
/// <param name="ItemCode">The item code.</param>
/// <param name="ItemName">The item name.</param>
/// <param name="RequiredQuantity">The quantity the order requires.</param>
/// <param name="ConsumedQuantity">The quantity consumed so far.</param>
/// <param name="LotTracked">Whether the item is lot-tracked, so the lot is mandatory.</param>
public sealed record MesComponent(Guid ItemId, string ItemCode, string ItemName, decimal RequiredQuantity, decimal ConsumedQuantity, bool LotTracked);
