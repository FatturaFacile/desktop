// <copyright file="RebateSettlementProposal.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A settlement proposal of the open accruals of a supplier rebate agreement.
/// </summary>
/// <param name="Id">The proposal identifier.</param>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="Amount">The net rebate to settle.</param>
/// <param name="GrossAmount">The gross total.</param>
/// <param name="TaxRate">The VAT rate in percent.</param>
/// <param name="TaxAmount">The VAT.</param>
/// <param name="Currency">The currency.</param>
/// <param name="Status">The workflow status.</param>
/// <param name="ExecutionMode">How the proposal is executed.</param>
/// <param name="PassiveInvoiceId">The payable the amount is set off against, for a set-off.</param>
/// <param name="SetOffAmount">The amount set off once executed.</param>
/// <param name="Version">The optimistic concurrency version.</param>
/// <param name="Lines">The analytical breakdown.</param>
public sealed record RebateSettlementProposal(
    string Id,
    string AgreementId,
    decimal Amount,
    decimal GrossAmount,
    decimal? TaxRate,
    decimal TaxAmount,
    string Currency,
    SettlementStatus Status,
    RebateExecutionMode? ExecutionMode,
    string? PassiveInvoiceId,
    decimal? SetOffAmount,
    int Version,
    IReadOnlyList<RebateSettlementLine> Lines)
{
    /// <summary>
    /// Gets the workflow actions the proposal accepts.
    /// </summary>
    public IReadOnlyList<SettlementAction> AllowedActions => SettlementWorkflow.AllowedActions(Status);
}
