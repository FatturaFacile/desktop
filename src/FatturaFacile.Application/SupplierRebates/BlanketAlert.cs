// <copyright file="BlanketAlert.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// An alert of a blanket agreement balance.
/// </summary>
/// <param name="Code">The alert code: ceiling_near, ceiling_exceeded, minimum_at_risk or minimum_not_met.</param>
/// <param name="Severity">The severity: warning or critical.</param>
public sealed record BlanketAlert(string Code, string Severity);
