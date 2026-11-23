// <copyright file="RebateContributionSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The contributions of an agreement by status.
/// </summary>
/// <param name="Planned">The planned contributions.</param>
/// <param name="Accrued">The accrued contributions.</param>
/// <param name="Settled">The settled contributions.</param>
public sealed record RebateContributionSummary(decimal Planned, decimal Accrued, decimal Settled);
