// <copyright file="HelpdeskTimeOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// How a logged time entry was settled: deducted from the contract hour bank, billable, or neither.
/// </summary>
/// <param name="BillingStatus">The billing status: contract_deducted, partially_deducted, billable_pending or non_billable.</param>
/// <param name="ContractHours">The hours deducted from the contract.</param>
/// <param name="BillableHours">The hours left to bill.</param>
/// <param name="TotalCharge">The charge of the billable hours.</param>
/// <param name="IsEstimate">Whether the figures are a local estimate (offline) rather than the server answer.</param>
public sealed record HelpdeskTimeOutcome(string BillingStatus, decimal ContractHours, decimal BillableHours, decimal TotalCharge, bool IsEstimate = false)
{
    /// <summary>
    /// The whole entry came out of the contract bank.
    /// </summary>
    public const string ContractDeducted = "contract_deducted";

    /// <summary>
    /// Part of the entry came out of the contract bank and the rest is billable.
    /// </summary>
    public const string PartiallyDeducted = "partially_deducted";

    /// <summary>
    /// The whole entry is billable.
    /// </summary>
    public const string BillablePending = "billable_pending";

    /// <summary>
    /// The entry is neither deducted nor billed.
    /// </summary>
    public const string NonBillable = "non_billable";
}
