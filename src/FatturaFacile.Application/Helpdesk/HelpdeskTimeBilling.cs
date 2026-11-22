// <copyright file="HelpdeskTimeBilling.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// Local estimate of how a time entry will be settled, shown while the entry waits in the outbox. The server decides; the
/// figures follow the same rule so the agent sees the likely outcome before connectivity returns.
/// </summary>
public static class HelpdeskTimeBilling
{
    private const decimal MinutesPerHour = 60m;
    private const int HourDecimals = 2;

    /// <summary>
    /// Estimates the settlement of a time entry against the hour bank of the contract.
    /// </summary>
    /// <param name="minutes">The minutes worked.</param>
    /// <param name="isBillable">Whether the entry may be billed.</param>
    /// <param name="remainingContractHours">The hours left on the contract bank, or null when there is no contract with a bank.</param>
    /// <returns>The estimated settlement; the charge is unknown offline and stays zero.</returns>
    public static HelpdeskTimeOutcome Estimate(int minutes, bool isBillable, decimal? remainingContractHours)
    {
        var hours = Math.Round(minutes / MinutesPerHour, HourDecimals, MidpointRounding.AwayFromZero);
        var bank = Math.Max(remainingContractHours ?? 0m, 0m);

        return (isBillable, bank) switch
        {
            (false, _) => new HelpdeskTimeOutcome(HelpdeskTimeOutcome.NonBillable, 0m, 0m, 0m, IsEstimate: true),
            (true, 0m) => new HelpdeskTimeOutcome(HelpdeskTimeOutcome.BillablePending, 0m, hours, 0m, IsEstimate: true),
            (true, _) when bank >= hours => new HelpdeskTimeOutcome(HelpdeskTimeOutcome.ContractDeducted, hours, 0m, 0m, IsEstimate: true),
            _ => new HelpdeskTimeOutcome(HelpdeskTimeOutcome.PartiallyDeducted, bank, hours - bank, 0m, IsEstimate: true),
        };
    }
}
