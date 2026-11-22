// <copyright file="FieldServiceWorkOrder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A field work order with its consumed parts and its service report.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Number">The number.</param>
/// <param name="WorkType">The type.</param>
/// <param name="Status">The status.</param>
/// <param name="Priority">The priority.</param>
/// <param name="Title">The title.</param>
/// <param name="Problem">The problem description.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="EquipmentId">The equipment.</param>
/// <param name="ContractId">The maintenance contract.</param>
/// <param name="TechnicianId">The assigned technician.</param>
/// <param name="Start">The scheduled start.</param>
/// <param name="End">The scheduled end.</param>
/// <param name="Address">The site address.</param>
/// <param name="RequiredSkills">The required skills.</param>
/// <param name="Parts">The consumed parts (empty in the list).</param>
/// <param name="Report">The service report, or null.</param>
/// <param name="InvoiceId">The invoice, if any.</param>
public sealed record FieldServiceWorkOrder(string Id, string Number, string WorkType, string Status, string Priority, string Title, string? Problem, string? CustomerId, string? EquipmentId, string? ContractId, string? TechnicianId, DateTimeOffset? Start, DateTimeOffset? End, string? Address, IReadOnlyList<string> RequiredSkills, IReadOnlyList<FieldServicePart> Parts, FieldServiceReport? Report, string? InvoiceId);
