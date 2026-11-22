// <copyright file="ProjectEvmReconciliation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// The managerial project cost against the statutory valuation (OIC 23 in Italy, Swiss GAAP FER 22 in Switzerland).
/// </summary>
/// <param name="Standard">The accounting standard of the tenant, or null.</param>
/// <param name="ManagerialCost">The managerial actual cost of the project.</param>
/// <param name="StatutoryCost">The cumulative cost of the statutory valuation.</param>
/// <param name="Difference">The cost difference, or null while there is no valuation.</param>
/// <param name="Overhead">The overhead taken from the cost centres.</param>
/// <param name="IsReconciled">Whether the managerial and the statutory cost agree.</param>
/// <param name="HasValuation">Whether a statutory valuation was recorded.</param>
public sealed record ProjectEvmReconciliation(
    string? Standard,
    double ManagerialCost,
    double StatutoryCost,
    double? Difference,
    double Overhead,
    bool IsReconciled,
    bool HasValuation);
