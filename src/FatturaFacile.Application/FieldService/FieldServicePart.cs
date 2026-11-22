// <copyright file="FieldServicePart.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A spare part consumed on a work order.
/// </summary>
/// <param name="Id">The consumption identifier (empty for a part still in the outbox).</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Description">The description.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="UnitPrice">The unit price charged.</param>
/// <param name="Covered">Whether the contract or the warranty covers the part, so it is not charged.</param>
/// <param name="IsPending">Whether the part is only in the local outbox and the server has not priced it yet.</param>
public sealed record FieldServicePart(string Id, string ProductId, string Description, decimal Quantity, decimal UnitPrice, bool Covered, bool IsPending);
