// <copyright file="FieldServiceReportInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The service report the customer signed, as the technician entered it.
/// </summary>
/// <param name="WorkOrderId">The work order.</param>
/// <param name="Summary">The work performed.</param>
/// <param name="Checklist">The checklist.</param>
/// <param name="LaborHours">The labor hours.</param>
/// <param name="TravelHours">The travel hours.</param>
/// <param name="TravelKm">The travel kilometres.</param>
/// <param name="ApplyCalloutFee">Whether the call-out fee applies.</param>
/// <param name="MeterHours">The operating hours read on the equipment.</param>
/// <param name="MeterCycles">The cycle count read on the equipment.</param>
/// <param name="SignerName">The customer signer.</param>
/// <param name="SignerRole">The role of the signer.</param>
/// <param name="SignatureSvg">The signature drawing as SVG.</param>
/// <param name="Locale">The language of the PDF (it-IT, it-CH, de-CH, fr-CH or en).</param>
public sealed record FieldServiceReportInput(Guid WorkOrderId, string Summary, IReadOnlyList<FieldServiceChecklistItem> Checklist, decimal LaborHours, decimal TravelHours, decimal TravelKm, bool ApplyCalloutFee, decimal? MeterHours, int? MeterCycles, string SignerName, string? SignerRole, string SignatureSvg, string Locale);
