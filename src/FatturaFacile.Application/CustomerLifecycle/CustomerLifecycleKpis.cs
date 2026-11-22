// <copyright file="CustomerLifecycleKpis.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// The cross-module KPIs of a customer (web-api customer 360 summary). Money amounts arrive as decimal strings.
/// </summary>
/// <param name="LifetimeValue">The gross amount of the posted invoices.</param>
/// <param name="OpenPipelineValue">The amount of the open opportunities.</param>
/// <param name="OpenOpportunities">The number of open opportunities.</param>
/// <param name="OverdueReceivableAmount">The overdue unpaid amount.</param>
/// <param name="OpenTickets">The number of unresolved tickets.</param>
/// <param name="SlaBreachRate">The share of tickets with a breached SLA, in percent.</param>
/// <param name="MttrHours">The mean time to resolution in hours, when a ticket was resolved.</param>
/// <param name="ActiveContracts">The number of active maintenance contracts.</param>
/// <param name="ContractCoverage">none, covered or exhausted.</param>
/// <param name="ContractHoursRemaining">The hours left on the active contracts.</param>
/// <param name="ContractsExpiringSoon">The active contracts ending within 60 days.</param>
/// <param name="InterventionsScheduled">The open field interventions.</param>
/// <param name="InterventionsCompleted">The completed field interventions.</param>
public sealed record CustomerLifecycleKpis(
    [property: JsonPropertyName("lifetime_value")] string LifetimeValue,
    [property: JsonPropertyName("open_pipeline_value")] string OpenPipelineValue,
    [property: JsonPropertyName("open_opportunities")] int OpenOpportunities,
    [property: JsonPropertyName("overdue_receivable_amount")] string OverdueReceivableAmount,
    [property: JsonPropertyName("open_tickets")] int OpenTickets,
    [property: JsonPropertyName("sla_breach_rate")] double SlaBreachRate,
    [property: JsonPropertyName("mttr_hours")] double? MttrHours,
    [property: JsonPropertyName("active_contracts")] int ActiveContracts,
    [property: JsonPropertyName("contract_coverage")] string ContractCoverage,
    [property: JsonPropertyName("contract_hours_remaining")] string ContractHoursRemaining,
    [property: JsonPropertyName("contracts_expiring_soon")] int ContractsExpiringSoon,
    [property: JsonPropertyName("interventions_scheduled")] int InterventionsScheduled,
    [property: JsonPropertyName("interventions_completed")] int InterventionsCompleted);
