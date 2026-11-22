// <copyright file="FieldServiceEquipment.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A customer asset with its meters and service dates.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="CustomerId">The owner.</param>
/// <param name="ContractId">The maintenance contract.</param>
/// <param name="SiteAddress">The site address.</param>
/// <param name="OperatingHours">The operating hours.</param>
/// <param name="CycleCount">The cycle count.</param>
/// <param name="LastServiceDate">The last service date.</param>
/// <param name="NextServiceDate">The next scheduled service date.</param>
public sealed record FieldServiceEquipment(string Id, string SerialNumber, string? CustomerId, string? ContractId, string? SiteAddress, decimal OperatingHours, int CycleCount, DateOnly? LastServiceDate, DateOnly? NextServiceDate);
