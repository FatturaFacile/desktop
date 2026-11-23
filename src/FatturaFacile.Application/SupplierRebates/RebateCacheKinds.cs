// <copyright file="RebateCacheKinds.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The kinds of payload kept in the local cache of the supplier rebates. A payload is addressed by its kind and a key (the
/// identifier of the agreement or blanket agreement, or <see cref="AllKey"/> for a list).
/// </summary>
public static class RebateCacheKinds
{
    /// <summary>
    /// The key of the payloads that are not tied to one record.
    /// </summary>
    public const string AllKey = "all";

    /// <summary>
    /// The list of the rebate agreements.
    /// </summary>
    public const string Agreements = "agreements";

    /// <summary>
    /// One agreement with its scale and contributions.
    /// </summary>
    public const string Agreement = "agreement";

    /// <summary>
    /// The live progress of one agreement.
    /// </summary>
    public const string Progress = "progress";

    /// <summary>
    /// The accrual runs of one agreement.
    /// </summary>
    public const string Runs = "runs";

    /// <summary>
    /// The settlement proposals of one agreement.
    /// </summary>
    public const string Proposals = "proposals";

    /// <summary>
    /// The list of the blanket agreements.
    /// </summary>
    public const string BlanketContracts = "blanket-contracts";

    /// <summary>
    /// The balance of one blanket agreement.
    /// </summary>
    public const string BlanketBalance = "blanket-balance";
}
