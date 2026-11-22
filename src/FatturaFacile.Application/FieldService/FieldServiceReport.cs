// <copyright file="FieldServiceReport.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The signed service report (Rapportino d'Intervento) of a work order.
/// </summary>
/// <param name="Id">The report identifier.</param>
/// <param name="WorkOrderId">The work order identifier.</param>
/// <param name="Number">The report number.</param>
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
/// <param name="SignatureIntact">False when the stored report no longer matches its SHA-256 seal.</param>
/// <param name="SignedAt">When the customer signed.</param>
/// <param name="Locale">The language of the PDF.</param>
/// <param name="HasPdf">Whether the server holds the PDF.</param>
/// <param name="InvoiceId">The invoice created from the report, if any.</param>
/// <param name="Billing">The billing breakdown.</param>
/// <param name="IsPending">Whether the report is only in the local outbox.</param>
public sealed record FieldServiceReport(string Id, string WorkOrderId, string Number, string Summary, IReadOnlyList<FieldServiceChecklistItem> Checklist, decimal LaborHours, decimal TravelHours, decimal TravelKm, bool ApplyCalloutFee, decimal? MeterHours, int? MeterCycles, string SignerName, string? SignerRole, bool SignatureIntact, DateTimeOffset SignedAt, string Locale, bool HasPdf, string? InvoiceId, FieldServiceBilling Billing, bool IsPending);
