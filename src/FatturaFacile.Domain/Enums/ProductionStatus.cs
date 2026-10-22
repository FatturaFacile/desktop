// <copyright file="ProductionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Defines the lifecycle execution stage of a production job, manufacturing order, or workshop assembly process.
/// </summary>
public enum ProductionStatus
{
    /// <summary>
    /// Scheduled and queued for future processing, with material requirements and capacity allocated but active execution not yet started.
    /// </summary>
    Planned = 1,

    /// <summary>
    /// Currently being processed on the production floor or workshop workstation, consuming labor, components, and machine time.
    /// </summary>
    InProduction = 2,

    /// <summary>
    /// Manufacturing operations completed, quality inspections passed, and output products ready for inventory intake or dispatch.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Aborted or revoked prior to completion due to engineering revisions, customer order cancellation, or defect detection.
    /// </summary>
    Cancelled = 4,
}
