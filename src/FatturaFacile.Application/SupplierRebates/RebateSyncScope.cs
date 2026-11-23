// <copyright file="RebateSyncScope.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// What a supplier rebate sync pulls.
/// </summary>
public enum RebateSyncScope
{
    /// <summary>
    /// The list of the rebate agreements.
    /// </summary>
    Agreements = 0,

    /// <summary>
    /// One agreement with its progress, runs and proposals.
    /// </summary>
    Agreement = 1,

    /// <summary>
    /// The list of the blanket agreements.
    /// </summary>
    BlanketContracts = 2,

    /// <summary>
    /// The balance of one blanket agreement.
    /// </summary>
    BlanketBalance = 3,
}
