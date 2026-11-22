// <copyright file="FieldServiceBlock.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A work order block of the dispatcher board: an entry of the unassigned queue or a scheduled bar of a technician lane.
/// </summary>
/// <param name="WorkOrderId">The work order identifier.</param>
/// <param name="Number">The work order number.</param>
/// <param name="Title">The title.</param>
/// <param name="WorkType">The type (preventive, corrective, installation, warranty).</param>
/// <param name="Priority">The priority (low, normal, high, urgent).</param>
/// <param name="Status">The status.</param>
/// <param name="Start">The scheduled start, or null when unscheduled.</param>
/// <param name="End">The scheduled end, or null when unscheduled.</param>
/// <param name="Address">The site address.</param>
/// <param name="Latitude">The site latitude.</param>
/// <param name="Longitude">The site longitude.</param>
/// <param name="RequiredSkills">The skills the visit needs (empty for scheduled bars).</param>
/// <param name="RequiredParts">The number of distinct spare parts the visit needs (zero for scheduled bars).</param>
public sealed record FieldServiceBlock(string WorkOrderId, string Number, string Title, string WorkType, string Priority, string Status, DateTimeOffset? Start, DateTimeOffset? End, string? Address, double? Latitude, double? Longitude, IReadOnlyList<string> RequiredSkills, int RequiredParts);
