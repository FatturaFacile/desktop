// <copyright file="ProjectBillingSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// Everything the project billing view shows for one project, as of the last sync.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="Summary">What the project has earned but not billed.</param>
/// <param name="Contract">The terms of the billing contract, when the project has one.</param>
/// <param name="Certificates">The SAL certificates, in number order.</param>
/// <param name="Retainer">The prepaid retainer, when the contract has one.</param>
/// <param name="Retention">The guarantee retention, when the project has a contract.</param>
/// <param name="SyncedAtUtc">When the snapshot was pulled.</param>
public sealed record ProjectBillingSnapshot(
    string ProjectId,
    ProjectBillingSummary Summary,
    ProjectBillingContractTerms? Contract,
    IReadOnlyList<ProjectBillingSalCertificate> Certificates,
    ProjectBillingRetainer? Retainer,
    ProjectBillingRetention? Retention,
    DateTime SyncedAtUtc)
{
    /// <summary>
    /// Gets the last certificate that is not cancelled: the base of the next one.
    /// </summary>
    public ProjectBillingSalCertificate? LastLive => Certificates.Where(c => c.IsLive).OrderByDescending(c => c.Number).FirstOrDefault();

    /// <summary>
    /// Previews the next certificate at a cumulative completion, with the terms of the contract and the live certificates.
    /// </summary>
    /// <param name="cumulativePercent">The cumulative completion, from 0 to 100.</param>
    /// <returns>The figures, or null when the contract is not billed by progress or the completion is not valid.</returns>
    public ProjectBillingSalPreview? PreviewNext(decimal cumulativePercent)
    {
        if (Contract is not { BillsProgress: true } terms)
        {
            return null;
        }

        var last = LastLive;

        return ProjectBillingSalCalculator.Preview(
            terms.ContractValue,
            cumulativePercent,
            last?.CumulativePercent ?? 0m,
            last?.CumulativeGross ?? 0m,
            terms.AdvanceTotal,
            Certificates.Where(c => c.IsLive).Sum(c => c.AdvanceDeduction),
            terms.EffectiveRetentionPercent);
    }
}
