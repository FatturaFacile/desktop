// <copyright file="MaintenanceContract.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A maintenance contract: its coverage, preventive rule, included visits and call-out fee.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Number">The contract number.</param>
/// <param name="Title">The title.</param>
/// <param name="StartDate">The start date.</param>
/// <param name="EndDate">The end date.</param>
/// <param name="Status">The status: draft, active, suspended, expired or cancelled.</param>
/// <param name="Coverage">The coverage type.</param>
/// <param name="Frequency">The preventive rule.</param>
/// <param name="IncludedVisits">The visits included per year.</param>
/// <param name="UsedVisits">The visits used in the current year.</param>
/// <param name="CalloutFee">The call-out fee.</param>
/// <param name="HourlyRate">The hourly labor rate.</param>
/// <param name="Currency">The currency.</param>
/// <param name="Notes">The notes.</param>
/// <param name="EquipmentIds">The covered equipment.</param>
public sealed record MaintenanceContract(string Id, string CustomerId, string Number, string Title, DateOnly StartDate, DateOnly? EndDate, string Status, MaintenanceCoverageType Coverage, PreventiveFrequency? Frequency, int? IncludedVisits, int UsedVisits, decimal CalloutFee, decimal? HourlyRate, string Currency, string? Notes, IReadOnlyList<string> EquipmentIds);
