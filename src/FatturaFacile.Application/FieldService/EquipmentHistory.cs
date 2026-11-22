// <copyright file="EquipmentHistory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The service history of a customer asset: service reports, replaced parts and meter progression.
/// </summary>
/// <param name="Id">The equipment identifier.</param>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="OperatingHours">The current operating hours.</param>
/// <param name="CycleCount">The current cycle count.</param>
/// <param name="LastServiceDate">The last service date.</param>
/// <param name="NextServiceDate">The next scheduled service date.</param>
/// <param name="Entries">The service reports, oldest first.</param>
public sealed record EquipmentHistory(string Id, string SerialNumber, decimal OperatingHours, int CycleCount, DateOnly? LastServiceDate, DateOnly? NextServiceDate, IReadOnlyList<EquipmentHistoryEntry> Entries);
