// <copyright file="ProjectBillingSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// What a project has earned but not billed.
/// </summary>
/// <param name="Currency">The currency code.</param>
/// <param name="ContractValue">The contract value.</param>
/// <param name="BilledToDate">The net revenue of the issued invoices.</param>
/// <param name="DraftBilling">The net amount of the draft invoices.</param>
/// <param name="UnbilledTimeAndMaterials">The approved billable hours and costs not yet invoiced.</param>
/// <param name="UnbilledMilestones">The completed milestones not yet invoiced.</param>
/// <param name="OpenSalCertificates">The SAL certificates still open.</param>
/// <param name="OpenSalAmount">The invoice amount of the open SAL certificates.</param>
/// <param name="RetentionHeld">The guarantee retention held, when the project has a contract.</param>
/// <param name="LatestWip">The latest WIP valuation, when there is one.</param>
public sealed record ProjectBillingSummary(string Currency, decimal ContractValue, decimal BilledToDate, decimal DraftBilling, decimal UnbilledTimeAndMaterials, int UnbilledMilestones, int OpenSalCertificates, decimal OpenSalAmount, decimal? RetentionHeld, ProjectBillingWip? LatestWip);
