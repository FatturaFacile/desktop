// <copyright file="BlanketBalance.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The live balance of a blanket purchase agreement: the ceiling and the guaranteed minimum spend against what was ordered, received
/// and invoiced.
/// </summary>
/// <param name="ContractId">The agreement identifier.</param>
/// <param name="Currency">The currency.</param>
/// <param name="CeilingAmount">The ceiling (plafond).</param>
/// <param name="MinimumSpendAmount">The guaranteed minimum spend, when there is one.</param>
/// <param name="OrderedAmount">The value of the call-off orders.</param>
/// <param name="ReceivedAmount">The value received.</param>
/// <param name="InvoicedAmount">The value invoiced.</param>
/// <param name="RemainingCeiling">The ceiling left, when there is a ceiling.</param>
/// <param name="CeilingUsedPercent">The share of the ceiling ordered, when there is a ceiling.</param>
/// <param name="MinimumShortfall">The spend missing to the minimum, when there is a minimum.</param>
/// <param name="MinimumReachedPercent">The share of the minimum ordered, when there is a minimum.</param>
/// <param name="Lines">The items of the agreement.</param>
/// <param name="Alerts">The alerts raised by the balance.</param>
public sealed record BlanketBalance(
    string ContractId,
    string Currency,
    decimal CeilingAmount,
    decimal? MinimumSpendAmount,
    decimal OrderedAmount,
    decimal ReceivedAmount,
    decimal InvoicedAmount,
    decimal? RemainingCeiling,
    decimal? CeilingUsedPercent,
    decimal? MinimumShortfall,
    decimal? MinimumReachedPercent,
    IReadOnlyList<BlanketBalanceLine> Lines,
    IReadOnlyList<BlanketAlert> Alerts);
