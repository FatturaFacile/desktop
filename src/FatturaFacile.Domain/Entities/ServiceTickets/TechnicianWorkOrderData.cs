// <copyright file="TechnicianWorkOrderData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.ServiceTickets;

/// <summary>
/// Data transfer object holding technician work details, labor duration, rates, and parts used for a work order.
/// </summary>
/// <param name="TenantId">The tenant id.</param>
/// <param name="WorkOrderNumber">The work order number.</param>
/// <param name="TicketId">The ticket id.</param>
/// <param name="TechnicianId">The technician id.</param>
/// <param name="ServiceDate">The service date.</param>
/// <param name="HoursWorked">The hours worked.</param>
/// <param name="HourlyRate">The hourly rate.</param>
/// <param name="Currency">The currency.</param>
/// <param name="Summary">The summary.</param>
/// <param name="SpareParts">The spare parts.</param>
public sealed record TechnicianWorkOrderData(
    Guid     TenantId,
    string   WorkOrderNumber,
    Guid     TicketId,
    Guid     TechnicianId,
    DateTime ServiceDate,
    decimal  HoursWorked,
    decimal  HourlyRate,
    Currency Currency,
    string   Summary,
    string?  SpareParts = null);
