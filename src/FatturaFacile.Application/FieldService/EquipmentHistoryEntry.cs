// <copyright file="EquipmentHistoryEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A signed service report of the history of an asset.
/// </summary>
/// <param name="ReportId">The report identifier.</param>
/// <param name="ReportNumber">The report number.</param>
/// <param name="WorkOrderId">The work order.</param>
/// <param name="WorkType">The work type.</param>
/// <param name="SignedAt">When it was signed.</param>
/// <param name="Summary">The work performed.</param>
/// <param name="LaborHours">The labor hours.</param>
/// <param name="MeterHours">The operating hours reading.</param>
/// <param name="MeterCycles">The cycle reading.</param>
/// <param name="Parts">The parts replaced.</param>
public sealed record EquipmentHistoryEntry(string ReportId, string ReportNumber, string WorkOrderId, string WorkType, DateTimeOffset SignedAt, string Summary, decimal LaborHours, decimal? MeterHours, int? MeterCycles, IReadOnlyList<EquipmentHistoryPart> Parts);
