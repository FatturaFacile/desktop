// <copyright file="FieldServiceBillingLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A priced line of a service report: labor, travel, call-out fee or part.
/// </summary>
/// <param name="Kind">The kind: labor, travel, callout or part.</param>
/// <param name="Description">The description.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="UnitPrice">The list unit price.</param>
/// <param name="ListAmount">The list amount.</param>
/// <param name="ChargedAmount">The amount charged (zero when covered).</param>
/// <param name="Covered">Whether the line is covered.</param>
public sealed record FieldServiceBillingLine(string Kind, string Description, decimal Quantity, decimal UnitPrice, decimal ListAmount, decimal ChargedAmount, bool Covered);
