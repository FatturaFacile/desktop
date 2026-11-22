// <copyright file="EquipmentHistoryPart.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A part replaced during a past visit.
/// </summary>
/// <param name="ProductId">The product.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="Covered">Whether it was covered.</param>
public sealed record EquipmentHistoryPart(string ProductId, decimal Quantity, bool Covered);
