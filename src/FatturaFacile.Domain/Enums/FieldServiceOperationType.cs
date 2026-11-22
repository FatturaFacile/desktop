// <copyright file="FieldServiceOperationType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The kind of field operation a technician queued while offline. The values map one to one to the `type` of the operations of
/// the web-api `POST /api/v1/fsm/mobile-sync` batch.
/// </summary>
public enum FieldServiceOperationType
{
    /// <summary>
    /// The technician moved the work order to en route or on site (`status_transition`).
    /// </summary>
    StatusTransition = 0,

    /// <summary>
    /// A spare part was taken from the van (`part_consumption`).
    /// </summary>
    PartConsumption = 1,

    /// <summary>
    /// The service report was signed by the customer and closed the work order (`service_report`).
    /// </summary>
    ServiceReport = 2,
}
