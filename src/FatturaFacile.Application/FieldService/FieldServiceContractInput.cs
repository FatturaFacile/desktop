// <copyright file="FieldServiceContractInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The fields of a maintenance contract being created or edited.
/// </summary>
/// <param name="Id">The identifier of the contract being edited, or null for a new one.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Number">The contract number.</param>
/// <param name="Title">The title.</param>
/// <param name="StartDate">The start date.</param>
/// <param name="EndDate">The end date.</param>
/// <param name="Status">The status.</param>
/// <param name="Coverage">The coverage type.</param>
/// <param name="Frequency">The preventive rule.</param>
/// <param name="IncludedVisits">The visits included per year.</param>
/// <param name="CalloutFee">The call-out fee.</param>
/// <param name="HourlyRate">The hourly labor rate.</param>
/// <param name="Currency">The currency.</param>
/// <param name="Notes">The notes.</param>
/// <param name="EquipmentIds">The covered equipment.</param>
public sealed record FieldServiceContractInput(string? Id, string CustomerId, string Number, string Title, DateOnly StartDate, DateOnly? EndDate, string Status, MaintenanceCoverageType Coverage, PreventiveFrequency? Frequency, int? IncludedVisits, decimal CalloutFee, decimal? HourlyRate, string Currency, string? Notes, IReadOnlyList<string> EquipmentIds);
