// <copyright file="FieldServiceBundleItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A work order of the offline bundle of a technician.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Number">The number.</param>
/// <param name="WorkType">The type.</param>
/// <param name="Priority">The priority.</param>
/// <param name="Status">The status.</param>
/// <param name="Title">The title.</param>
/// <param name="Start">The scheduled start.</param>
/// <param name="End">The scheduled end.</param>
/// <param name="Address">The site address.</param>
/// <param name="RequiredSkills">The required skills.</param>
/// <param name="Checklist">The checklist template items.</param>
/// <param name="CustomerName">The customer name.</param>
/// <param name="EquipmentSerial">The serial number of the equipment.</param>
public sealed record FieldServiceBundleItem(string Id, string Number, string WorkType, string Priority, string Status, string Title, DateTimeOffset? Start, DateTimeOffset? End, string? Address, IReadOnlyList<string> RequiredSkills, IReadOnlyList<string> Checklist, string? CustomerName, string? EquipmentSerial);
